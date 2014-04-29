using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Linq;
using System.Diagnostics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
namespace Gallery
{
    [TemplatePart(Name = ElementScrollViewerName, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = ElementItemsPresenterName, Type = typeof(ItemsPresenter))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(FlipViewItem))]
    public class FlipView : TemplatedItemsControl<FlipViewItem>, ISupportInitialize
    {
        private const string ElementScrollViewerName = "ScrollViewer";
        private const string ElementItemsPresenterName = "ItemsPresenter";

        private const double CompressLimit = 125;
        private static readonly Duration ZeroDuration = TimeSpan.Zero;
        private static readonly Duration DefaultDuration = TimeSpan.FromSeconds(0.5);

        private readonly IEasingFunction _easingFunction = new ExponentialEase { Exponent = 5 };

        private InitializingData _initializingData;
        private bool _updatingSelection;

        private Orientation _orientation = Orientation.Horizontal;
        private Size _itemsHostSize = new Size(double.NaN, double.NaN);
        private List<FlipViewItem> _realizedItems = new List<FlipViewItem>();
        private bool _loaded;

        private AnimationDirection? _animationHint;
        private bool _animating;
        private bool _isEffectiveDragging;
        private bool _dragging;
        private DragLock _dragLock;
        private WeakReference _gestureSource;
        private Point _gestureOrigin;
        private Animator _panAnimator;
        private int? _deferredSelectedIndex;




        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.FlipView" /> class.
        /// </summary>
        public FlipView()
        {
            DefaultStyleKey = typeof(FlipView);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            Subscribe();


        }

        void FlipView_DoubleTap(object sender, GestureEventArgs e)
        {
            ShowAndHideBorder(e);
            TotalImageScale = 1;
            if (img == null)
            {
                img = InternalUtils.FindChild<Image>(InternalUtils.GetParentByType<Grid>(e.OriginalSource as DependencyObject,string.Empty), "PATH_Preview");
            }
            if (General_Pinch)
            {
                General_Pinch = false;
            }
            if (img != null)
            {
                ApplyScale();
            }
           
        }


        ~FlipView()
        {
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;

        }

        private void Subscribe()
        {
            ManipulationStarted += OnManipulationStarted;
            ManipulationDelta += OnManipulationDelta;
            ManipulationCompleted += OnManipulationCompleted;
            Tap += FlipView_Tap;
            DoubleTap += FlipView_DoubleTap;
        }

        void FlipView_Tap(object sender, GestureEventArgs e)
        {
           
            ShowAndHideBorder(e);
        }

       
        private void ShowAndHideBorder(GestureEventArgs e)
        {
            var border = InternalUtils.FindChild<Border>((e.OriginalSource as DependencyObject).GetParentByType<Grid>("GridImage"), "BottomBorder");
            if (border != null)
            {
                if (border.Opacity == 0)
                {
                    border.Opacity = 1;
                }
                else border.Opacity = 0;
            }
        }

        private void Unsubscribe()
        {
            ManipulationStarted -= OnManipulationStarted;
            ManipulationDelta -= OnManipulationDelta;
            ManipulationCompleted -= OnManipulationCompleted;
        }

        // these two fully define the zoom state:
        private double TotalImageScale = 1d;

        private const double MAX_IMAGE_ZOOM = 2.5;


        private void ResetImagePosition()
        {
            Pinch = false;
            TotalImageScale = 1;
            ApplyScale();
        }

        private void ApplyScale()
        {
            CreateAndApplyStoryboard(
                img,
             (img.RenderTransform as CompositeTransform).ScaleX,
             1,
              (img.RenderTransform as CompositeTransform).TranslateX * TotalImageScale,
             img.RenderTransformOrigin.X,
           (img.RenderTransform as CompositeTransform).TranslateY * TotalImageScale,
            img.RenderTransformOrigin.Y,
            10);




        }


        Storyboard sb;
        public void CreateAndApplyStoryboard(UIElement targetElement, double from, double to, double Xfrom, double Xto, double Yfrom, double Yto, double Velocity)
        {
            sb = new Storyboard { FillBehavior = FillBehavior.HoldEnd };
            sb.SkipToFill();


            ExponentialEase ee = new ExponentialEase();
            ee.EasingMode = EasingMode.EaseInOut;
            ee.Exponent = 3;

            #region
            DoubleAnimationUsingKeyFrames positionX = new DoubleAnimationUsingKeyFrames()
            {
                BeginTime = TimeSpan.Zero
            };

            positionX.Duration = new Duration(new TimeSpan(0, 0, 1));
            Storyboard.SetTargetProperty(positionX, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            Storyboard.SetTarget(positionX, targetElement);
            positionX.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = from, EasingFunction = ee });
            positionX.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 1), Value = to, EasingFunction = ee });
            sb.Children.Add(positionX);


            DoubleAnimationUsingKeyFrames positionY = new DoubleAnimationUsingKeyFrames()
            {
                BeginTime = TimeSpan.Zero
            };

            positionY.Duration = new Duration(new TimeSpan(0, 0, 1));
            Storyboard.SetTargetProperty(positionY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            Storyboard.SetTarget(positionY, targetElement);
            positionY.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = from, EasingFunction = ee });
            positionY.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 1), Value = to, EasingFunction = ee });
            sb.Children.Add(positionY);
            #endregion





            DoubleAnimationUsingKeyFrames dbz = new DoubleAnimationUsingKeyFrames()
            {
                BeginTime = TimeSpan.Zero
            };

            dbz.Duration = new Duration(new TimeSpan(0, 0, 1));
            Storyboard.SetTargetProperty(dbz, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            Storyboard.SetTarget(dbz, targetElement);
            dbz.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = from, EasingFunction = ee });
            dbz.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 1), Value = to, EasingFunction = ee });
            sb.Children.Add(dbz);




            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames()
            {
                BeginTime = TimeSpan.Zero
            };

            animation.Duration = new Duration(new TimeSpan(0, 0, 1));




            sb.SpeedRatio = 1;
            Storyboard.SetTarget(animation, targetElement);

            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));

            Storyboard.SetTarget(animation, targetElement);
            animation.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = TimeSpan.Zero, Value = from, EasingFunction = ee });
            animation.KeyFrames.Add(new EasingDoubleKeyFrame() { KeyTime = new TimeSpan(0, 0, 1), Value = to, EasingFunction = ee });

            sb.Children.Add(animation);
            sb.Begin();


        }










        bool Bang = false;


        private bool IsDragValid(double scaleDelta, Point translateDelta, Image img)
        {

            GeneralTransform gt = this.TransformToVisual(img);
            Point currentPos = gt.Transform(new Point(0, 0));




            var actualPosition = (img.ActualWidth + Math.Abs(currentPos.X) + translateDelta.X) * TotalImageScale;



            if (img.ActualWidth * TotalImageScale < this.ActualWidth)
            {
                if (actualPosition > this.ActualWidth)
                {


                    return false;
                }
            }
            else
            {
                actualPosition = img.ActualWidth * TotalImageScale - this.ActualWidth - Math.Abs(currentPos.X) * TotalImageScale;

                if (actualPosition < 0 && currentPos.X > 0)
                {


                    return false;

                }
                if (currentPos.X < 0)
                {

                    return false;
                }

            }
            return true;

        }










        #region IsPinch

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The index of the selected item. The default value is -1.
        /// </returns>
        public static readonly DependencyProperty InvertPinchProperty = DependencyProperty.RegisterAttached(
          "InvertPinch",
          typeof(bool),
          typeof(FlipView),
          new PropertyMetadata(true, OnInvertPinchChanged));

        private static void OnInvertPinchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var t = d as FlipView;
            Debug.WriteLine((bool)e.NewValue);
            if (t.InvertPinch != (bool)e.NewValue)
            {


                t.InvertPinch = (bool)e.NewValue;
            }
        }

        public bool InvertPinch
        {
            get { return (bool)GetValue(InvertPinchProperty); }
            set { SetValue(InvertPinchProperty, value); }
        }




        public bool Pinch
        {
            get { return (bool)GetValue(PinchProperty); }
            set
            {
                if (Pinch != value)
                {
                    InvertPinch = !value;
                }
                SetValue(PinchProperty, value);

            }
        }

        public static readonly DependencyProperty PinchProperty = DependencyProperty.RegisterAttached(
          "IsPinch",
          typeof(bool),
          typeof(FlipView),
          new PropertyMetadata(false, OnPinchChanged));



        private static void OnPinchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlipViewItem container = d as FlipViewItem;



            if (container != null)
            {
                //  container.OnIsPinchChanged((bool)e.NewValue);
            }
        }



        #endregion


        #region SelectedIndex

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The index of the selected item. The default is -1.
        /// </returns>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }




        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedIndex"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedIndex"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex",
            typeof(int),
            typeof(FlipView),
            new PropertyMetadata(-1, (d, e) => ((FlipView)d).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue)));

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        private void OnSelectedIndexChanged(int oldIndex, int newIndex)
        {
            if (_updatingSelection || IsInit)
            {
                return;
            }

            if (newIndex >= -1 && newIndex < Items.Count)
            {
                UpdateSelection(oldIndex, newIndex, SelectedItem, Items[newIndex]);
            }
            else
            {
                SelectedIndex = oldIndex;

                throw new ArgumentOutOfRangeException("SelectedIndex");
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// 
        /// <returns>
        /// The selected item. The default is null.
        /// </returns>
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.FlipView.SelectedItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem",
            typeof(object),
            typeof(FlipView),
             new PropertyMetadata((d, e) => ((FlipView)d).OnSelectedItemChanged(e.OldValue, e.NewValue)));
        //new PropertyMetadata((d, e) => ((FlipView)d).OnSelectedItemChanged(e.OldValue, e.NewValue)));

        private void OnSelectedItemChanged(object oldValue, object newValue)
        {
            //if (_updatingSelection || IsInit)
            //{
            //    return;
            //}

            int index = Items.IndexOf(newValue);
            if (index != -1 || (newValue == null && Items.Count == 0))
            {
                UpdateSelection(SelectedIndex, index, oldValue, newValue);
            }
            else
            {
                SelectedItem = oldValue;
            }
        }

        #endregion

        #region IsSelected

        internal static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(FlipView),
            new PropertyMetadata(OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FlipViewItem container = d as FlipViewItem;
            if (container != null)
            {
                container.OnIsSelectedChanged((bool)e.NewValue);
            }
        }

        #endregion

        private Panel ItemsHost { get; set; }

        private Size ItemsHostSize
        {
            get { return _itemsHostSize; }
            set
            {
                if (_itemsHostSize != value)
                {
                    _itemsHostSize = value;
                    UpdateItemsSize();
                }
            }
        }

        private ScrollViewer ElementScrollViewer { get; set; }

        private ItemsPresenter ElementItemsPresenter { get; set; }

        private Orientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    UpdateItemsSize();
                }
            }
        }

        private bool IsInit
        {
            get { return _initializingData != null; }
        }

        private bool ShouldHandleManipulation
        {
            get { return Items.Count > 1 && ElementItemsPresenter != null; }
        }

        /// <summary>
        /// Occurs when the currently selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.FlipView"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _panAnimator = null;
            ItemsHost = null;
            ItemsHostSize = new Size(double.NaN, double.NaN);

            if (ElementItemsPresenter != null)
            {
                LayoutUpdated -= OnLayoutUpdated;
            }

            base.OnApplyTemplate();

            ElementScrollViewer = GetTemplateChild(ElementScrollViewerName) as ScrollViewer;

            ElementItemsPresenter = GetTemplateChild(ElementItemsPresenterName) as ItemsPresenter;


            if (ElementItemsPresenter != null)
            {
                InitializeItemsHost();

                if (ItemsHost == null)
                {
                    LayoutUpdated += OnLayoutUpdated;
                }
            }



            // ElementItemsPresenter.AddHandler(OnDragDelta, _realizedItems[SelectedIndex].FlipViewItem_ManipulationDelta(VisualTreeHelper.GetChild(_realizedItems[SelectedIndex], 0)), true);
        }

        /// <summary>
        /// Handles the measure pass for the control.
        /// </summary>
        /// 
        /// <returns>
        /// The desired size.
        /// </returns>
        /// <param name="availableSize">The available size.</param>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsNaN(_itemsHostSize.Width) && double.IsNaN(_itemsHostSize.Height))
            {
                _itemsHostSize = availableSize;
                UpdateItemsSize();
            }

            return base.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Provides handling for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            int oldSelectedIndex = SelectedIndex;
            int newSelectedIndex = oldSelectedIndex;
            object oldSelectedItem = SelectedItem;
            object newSelectedItem = oldSelectedItem;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int index = 0; index < e.NewItems.Count; ++index)
                    {
                        FlipViewItem container = e.NewItems[index] as FlipViewItem;
                        if (container != null && container.IsSelected)
                        {
                            newSelectedIndex = e.NewStartingIndex + index;
                            newSelectedItem = container;
                        }
                    }

                    if (newSelectedIndex == oldSelectedIndex && e.NewStartingIndex <= oldSelectedIndex && !IsInit)
                    {
                        newSelectedIndex = oldSelectedIndex + e.NewItems.Count;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Contains(oldSelectedItem))
                    {
                        if (e.OldStartingIndex <= Items.Count - 1)
                        {
                            newSelectedIndex = e.OldStartingIndex;
                            newSelectedItem = Items[newSelectedIndex];
                        }
                        else if (Items.Count > 0)
                        {
                            newSelectedIndex = 0;
                            newSelectedItem = Items[0];
                        }
                        else
                        {
                            newSelectedIndex = -1;
                            newSelectedItem = null;
                        }
                    }
                    else if (e.OldStartingIndex + e.OldItems.Count <= oldSelectedIndex)
                    {
                        newSelectedIndex -= e.OldItems.Count;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems.Contains(oldSelectedItem))
                    {
                        FlipViewItem container = GetContainer(oldSelectedIndex) as FlipViewItem;
                        if (container != null)
                        {
                            container.IsSelected = true;
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (Items.Count > 0)
                    {
                        newSelectedIndex = 0;
                        newSelectedItem = Items[0];

                        if (ItemTemplate == null)
                        {
                            for (int index = 0; index < Items.Count; ++index)
                            {
                                FlipViewItem container = GetContainer(index);
                                if (container != null && container.IsSelected)
                                {
                                    newSelectedIndex = index;
                                    newSelectedItem = Items[index];
                                }
                            }
                        }
                    }
                    else
                    {
                        newSelectedIndex = -1;
                        newSelectedItem = null;
                    }
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (newSelectedIndex < 0 && Items.Count > 0)
            {
                newSelectedIndex = 0;
                newSelectedItem = Items[0];
            }

            UpdateSelection(oldSelectedIndex, newSelectedIndex, oldSelectedItem, newSelectedItem);
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">The element used to display the specified item.</param>
        /// <param name="item">The item to display</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            FlipViewItem container = (FlipViewItem)element;

            container.ParentFlipView = this;
            if (!ReferenceEquals(element, item))
            {
                container.Item = item;
            }

            int index = ItemContainerGenerator.IndexFromContainer(element);
            if (index != -1)
            {
                container.IsSelected = SelectedIndex == index;
            }

            _realizedItems.Add(container);

            UpdateItemSize(container, Orientation == Orientation.Horizontal);
        }

        /// <summary>
        /// Removes any bindings and templates applied to the item container for the specified content.
        /// </summary>
        /// <param name="element">The combo box item used to display the specified content.</param>
        /// <param name="item">The item content.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            FlipViewItem container = (FlipViewItem)element;

            if (!container.Equals(item))
            {
                container.ClearValue(ContentControl.ContentProperty);

            }

            container.ContentTemplate = null;
            container.Item = null;

            _realizedItems.Remove(container);
        }

        internal void NotifyItemSelected(FlipViewItem container, bool isSelected)
        {
            if (_updatingSelection)
            {
                return;
            }

            int index = ItemContainerGenerator.IndexFromContainer(container);
            if (index < 0 || index >= Items.Count)
            {
                return;
            }

            object item = container.Item ?? container;
            if (isSelected)
            {
                UpdateSelection(SelectedIndex, index, SelectedItem, item);
            }
            else
            {
                if (SelectedIndex == index)
                {
                    UpdateSelection(SelectedIndex, -1, SelectedItem, null);
                }
            }
        }

        private void InvokeSelectionChanged(List<object> unselectedItems, List<object> selectedItems)
        {
            OnSelectionChanged(new SelectionChangedEventArgs(unselectedItems, selectedItems));
        }

        private void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }

        private void OnSelectionChanged()
        {
            if (_animating)
            {
                GoTo(0, ZeroDuration);

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;
            }

            if (_panAnimator != null)
            {
                _panAnimator.GoTo(0, ZeroDuration);
            }

            ScrollSelectionIntoView();
        }

        private FlipViewItem GetContainer(int index)
        {
            if (index < 0 || Items.Count <= index)
            {
                return null;
            }
            else
            {
                return Items[index] as FlipViewItem ?? ItemContainerGenerator.ContainerFromIndex(index) as FlipViewItem;
            }
        }

        private void SetItemIsSelected(object item, bool value)
        {
            FlipViewItem container = item as FlipViewItem ?? ItemContainerGenerator.ContainerFromItem(item) as FlipViewItem;
            if (container != null)
            {
                container.IsSelected = value;
            }
        }

        private void UpdateSelection(int oldSelectedIndex, int newSelectedIndex, object oldSelectedItem, object newSelectedItem)
        {
            if (oldSelectedIndex == newSelectedIndex && InternalUtils.AreValuesEqual(oldSelectedItem, newSelectedItem))
            {
                return;
            }

            try
            {
                _updatingSelection = true;

                if (newSelectedIndex < 0 && Items.Count > 0)
                {
                    newSelectedIndex = 0;
                    newSelectedItem = Items[newSelectedIndex];
                }

                SelectedIndex = newSelectedIndex;
                SelectedItem = newSelectedItem;
                OnSelectionChanged();

                if (!InternalUtils.AreValuesEqual(oldSelectedItem, newSelectedItem))
                {
                    List<object> unselected = new List<object>();
                    List<object> selected = new List<object>();

                    if (oldSelectedItem != null)
                    {
                        SetItemIsSelected(oldSelectedItem, false);
                        unselected.Add(oldSelectedItem);
                    }

                    if (newSelectedItem != null)
                    {
                        SetItemIsSelected(newSelectedItem, true);
                        selected.Add(newSelectedItem);
                    }

                    InvokeSelectionChanged(unselected, selected);
                }
            }
            finally
            {
                _updatingSelection = false;
            }
        }

        private void UpdateItemsHostSize()
        {
            ItemsHostSize = new Size(ItemsHost.ActualWidth, ItemsHost.ActualHeight);
        }

        private void UpdateItemsSize()
        {
            bool horizontal = Orientation == Orientation.Horizontal;

            foreach (FlipViewItem container in _realizedItems)
            {
                UpdateItemSize(container, horizontal);
            }
        }

        private void UpdateItemSize(FlipViewItem container, bool horizontal)
        {
            if (horizontal)
            {
                container.Width = _itemsHostSize.Width;
                container.ClearValue(FrameworkElement.HeightProperty);
            }
            else
            {
                container.ClearValue(FrameworkElement.WidthProperty);
                container.Height = _itemsHostSize.Height;
            }
        }

        private void InitializeItemsHost()
        {
            ItemsHost = ElementItemsPresenter.GetFirstLogicalChildByType<Panel>(false);
            if (ItemsHost != null)
            {
                UpdateItemsHostSize();
                ItemsHost.SizeChanged += OnItemsHostSizeChanged;

                VirtualizingStackPanel vsp = ItemsHost as VirtualizingStackPanel;
                if (vsp != null)
                {
                    Orientation = vsp.Orientation;
                }
                else
                {

                    // throw new InvalidOperationException(Properties.Resources.FlipView_NotAllowedItemsPanel);
                }

                ScrollSelectionIntoView();
            }
        }

        private void OnItemsHostSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsHostSize();
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            InitializeItemsHost();

            if (ItemsHost != null)
            {
                LayoutUpdated -= OnLayoutUpdated;
            }
        }

        private void ScrollSelectionIntoView()
        {
            int index = SelectedIndex;

            if (ItemsHost != null && ElementScrollViewer != null && _loaded && index >= 0)
            {
                ElementScrollViewer.UpdateLayout();

                if (Orientation == Orientation.Horizontal)
                {
                    ElementScrollViewer.ScrollToHorizontalOffset(index);
                }
                else
                {
                    if (ElementScrollViewer.ViewportHeight != 1)
                    {
                        for (int i = 0; i < ElementScrollViewer.ExtentHeight; i++)
                        {
                            ElementScrollViewer.ScrollToVerticalOffset(i);
                            ElementScrollViewer.UpdateLayout();

                            if (ElementScrollViewer.ViewportHeight == 1)
                            {
                                break;
                            }
                        }
                    }

                    ElementScrollViewer.ScrollToVerticalOffset(index);
                }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            ScrollSelectionIntoView();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loaded = false;
        }

        private bool GlobalPinch = false;

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {


            img = InternalUtils.FindChild<Image>(InternalUtils.GetParentByType<Grid>(e.OriginalSource as DependencyObject,string.Empty), "PATH_DesireImage");
            GlobalPinch = false;
        }
        Image img;
        bool General_Pinch = false;
        /// <summary>
        /// Calculating Clip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {

            if (BangDone)
            {
                var transform = (CompositeTransform)img.RenderTransform;

                var translationDelta = new Point(e.DeltaManipulation.Translation.X * TotalImageScale, e.DeltaManipulation.Translation.Y * TotalImageScale);
                transform.TranslateX += translationDelta.X;
                transform.TranslateY += translationDelta.Y;
                if (IsDragValid(1, translationDelta, img))
                {
                    Bang = false;
                    BangDone = false;

                    return;
                }
                else
                {
                    ResetImagePosition();
                    Bang = false;
                    General_Pinch = false;
                    BangDone = false;

                }
            }

            if (!General_Pinch)
            {
                if (!_dragging)
                {
                    ReleaseMouseCaptureAtGestureOrigin();
                }

                _dragging = true;

                if (_dragLock == DragLock.Unset)
                {
                    double angle = AngleFromVector(e.CumulativeManipulation.Translation.X, e.CumulativeManipulation.Translation.Y) % 180;
                    _dragLock = angle <= 45 || angle >= 135 ? DragLock.Horizontal : DragLock.Vertical;
                }

                //e.Handled = true;

                if (_dragLock == DragLock.Horizontal && e.DeltaManipulation.Translation.X != 0 && Orientation == Orientation.Horizontal ||
                    _dragLock == DragLock.Vertical && e.DeltaManipulation.Translation.Y != 0 && Orientation == Orientation.Vertical)
                {
                    Drag(e);
                }
                if (e.PinchManipulation != null && img!=null && (img.ActualHeight!=0 || img.ActualWidth!=0))
                {
                    General_Pinch = true;
                }
            }
            else
            {
                if (!this._isEffectiveDragging)
                {
                    img = InternalUtils.FindChild<Image>(InternalUtils.GetParentByType<Grid>(e.OriginalSource as DependencyObject,string.Empty), "PATH_DesireImage");
                }
                else
                {
                    General_Pinch = false;
                    return;
                }

                if (e.PinchManipulation != null && img != null)
                {
                    General_Pinch = true;

                    var transform = (CompositeTransform)img.RenderTransform;


                    TotalImageScale *= e.PinchManipulation.DeltaScale;

                    if (TotalImageScale > 1.0 && TotalImageScale < MAX_IMAGE_ZOOM)
                    {
                        // Scale Manipulation

                        transform.ScaleX = TotalImageScale;
                        transform.ScaleY = TotalImageScale;

                        GeneralTransform gt = this.TransformToVisual(img);

                        Point currentPos = gt.Transform(new Point(0, 0));




                        transform.CenterX =Math.Abs((currentPos.X-img.ActualWidth)/2);

                      
                        transform.CenterY = Math.Abs(currentPos.Y-img.ActualHeight)/2;

                        Debug.WriteLine(currentPos);
                    }

                    else
                    {
                        TotalImageScale /= e.PinchManipulation.DeltaScale;
                    }

                    e.Handled = true;
                    return;
                }

                if (General_Pinch && img != null)
                {

                    var transform = (CompositeTransform)img.RenderTransform;

                    var translationDelta = new Point(e.DeltaManipulation.Translation.X * TotalImageScale, e.DeltaManipulation.Translation.Y * TotalImageScale);

                    if (IsDragValid(1, translationDelta, img))
                    {
                        Bang = false;
                        BangDone = false;
                        transform.TranslateX += e.DeltaManipulation.Translation.X;
                        transform.TranslateY += e.DeltaManipulation.Translation.Y;

                    }
                    else
                    {
                        Bang = true;
                    }
                    e.Handled = true;
                    return;
                }
            }

        }
        private bool BangDone = false;
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            img = null;
            if (!General_Pinch)
            {
                ManipulationDelta totalManipulation = null;

                _dragLock = DragLock.Unset;
                _dragging = false;

                if (e.IsInertial)
                {
                    double angle = AngleFromVector(e.FinalVelocities.LinearVelocity.X, e.FinalVelocities.LinearVelocity.Y);

                    if (Orientation == Orientation.Vertical)
                    {
                        angle -= 90;
                        if (angle < 0)
                        {
                            angle += 360;
                        }
                    }

                    if (angle <= 45 || angle >= 315)
                    {
                        angle = 0;
                    }
                    else if (angle >= 135 && angle <= 225)
                    {
                        angle = 180;
                    }

                    ReleaseMouseCaptureAtGestureOrigin();

                    Flick(angle);

                    if (angle == 0 || angle == 180)
                    {
                        e.Handled = true;
                    }
                }
                else if (e.TotalManipulation.Translation.X != 0 || e.TotalManipulation.Translation.Y != 0)
                {
                    totalManipulation = e.TotalManipulation;

                    if (_isEffectiveDragging)
                    {
                        e.Handled = true;
                    }
                }

                GesturesComplete(totalManipulation);
            }
            else
            {
                if (Bang)
                {
                    BangDone = true;
                }
            }
            //SetImgZoom(e.OriginalSource);


        }

        private double CalculateContentDestination(AnimationDirection direction)
        {
            double destination = 0;
            double itemSize = Orientation == Orientation.Horizontal ? _itemsHostSize.Width : _itemsHostSize.Height;
            switch (direction)
            {
                case AnimationDirection.Previous:
                    destination = -itemSize;
                    break;
                case AnimationDirection.Next:
                    destination = itemSize;
                    break;
            }
            return destination;
        }

        private void GesturesComplete(ManipulationDelta totalManipulation)
        {
            if (ShouldHandleManipulation)
            {
                if (totalManipulation != null && _isEffectiveDragging)
                {
                    bool horizontal = Orientation == Orientation.Horizontal;
                    double translation = horizontal ? totalManipulation.Translation.X : totalManipulation.Translation.Y;
                    double absoluteTranslation = Math.Abs(translation);
                    double itemSize = horizontal ? _itemsHostSize.Width : _itemsHostSize.Height;
                    if (translation != 0 && absoluteTranslation >= itemSize / 2)
                    {
                        NavigateByIndexChange(translation < 0 ? 1 : -1);
                    }
                }

                if (!_animating)
                {
                    GoTo(CalculateContentDestination(AnimationDirection.Center), DefaultDuration, _easingFunction);
                }
            }

            _isEffectiveDragging = false;
        }

        private void Flick(double angle)
        {
            if (ShouldHandleManipulation)
            {
                int intAngle = (int)angle;
                switch (intAngle)
                {
                    case 0:
                    case 180:
                        NavigateByIndexChange(intAngle == 180 ? 1 : -1);
                        break;
                }
            }
        }

        private void Drag(ManipulationDeltaEventArgs e)
        {
            _isEffectiveDragging = true;

            if (_animating || !ShouldHandleManipulation)
            {
                return;
            }

            double targetOffset = Orientation == Orientation.Horizontal ? e.CumulativeManipulation.Translation.X : e.CumulativeManipulation.Translation.Y;

            if (SelectedIndex <= 0)
            {
                if (targetOffset > CompressLimit)
                {
                    targetOffset = CompressLimit;
                }
            }
            else if (SelectedIndex >= Items.Count - 1)
            {
                if (targetOffset < -CompressLimit)
                {
                    targetOffset = -CompressLimit;
                }
            }

            GoTo(targetOffset, ZeroDuration);
        }

        private void NavigateByIndexChange(int indexDelta)
        {
            if (_animating)
            {
                GoTo(CalculateContentDestination(_animationHint.Value), ZeroDuration);

                int newSelectedIndex = _deferredSelectedIndex.Value;

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;

                SelectedIndex = newSelectedIndex;
            }
            bool end = false;
            if (indexDelta == 1)
            {
                if (SelectedIndex == Items.Count - 1)
                {
                    //if Cycling set SelectedIndex=0;
                    //end = true;
                    //_deferredSelectedIndex = 0;
                    //SelectedIndex = 0;
                    return;
                }
            }
            else
            {
                if (SelectedIndex == 0)
                {
                    return;
                }
            }

            _animationHint = indexDelta > 0 ? AnimationDirection.Previous : AnimationDirection.Next;
            _animating = true;
            //if(!end)
            _deferredSelectedIndex = SelectedIndex + indexDelta;

            GoTo(CalculateContentDestination(_animationHint.Value), DefaultDuration, _easingFunction, () =>
            {
                int newSelectedIndex = _deferredSelectedIndex.Value;

                _animationHint = null;
                _animating = false;
                _deferredSelectedIndex = null;

                SelectedIndex = newSelectedIndex;
            });
        }

        private void ReleaseMouseCaptureAtGestureOrigin()
        {
            if (_gestureSource != null)
            {
                FrameworkElement gestureSource = _gestureSource.Target as FrameworkElement;
                if (gestureSource != null)
                {
                    foreach (UIElement element in VisualTreeHelper.FindElementsInHostCoordinates(
                            gestureSource.TransformToVisual(null).Transform(_gestureOrigin), Application.Current.RootVisual))
                    {
                        element.ReleaseMouseCapture();
                    }
                }
            }
        }

        private void GoTo(double targetOffset, Duration duration)
        {
            GoTo(targetOffset, duration, null, null);
        }

        private void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
        {
            GoTo(targetOffset, duration, easingFunction, null);
        }

        private void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
        {
            if (Animator.TryEnsureAnimator(ElementItemsPresenter, Orientation, ref _panAnimator))
            {
                _panAnimator.GoTo(targetOffset, duration, easingFunction, completionAction);
            }
        }

        private static double AngleFromVector(double x, double y)
        {
            double num = Math.Atan2(y, x);
            if (num < 0)
            {
                num = 2 * Math.PI + num;
            }
            return num * 360 / (2 * Math.PI);
        }

        void ISupportInitialize.BeginInit()
        {
            _initializingData = new InitializingData
            {
                InitialItem = SelectedItem,
                InitialIndex = SelectedIndex
            };
        }

        void ISupportInitialize.EndInit()
        {
            if (_initializingData == null)
            {
                throw new InvalidOperationException();
            }

            int selectedIndex = SelectedIndex;
            object selectedItem = SelectedItem;

            if (_initializingData.InitialIndex != selectedIndex)
            {
                SelectedIndex = _initializingData.InitialIndex;
                _initializingData = null;
                SelectedIndex = selectedIndex;
            }
            else if (!ReferenceEquals(_initializingData.InitialItem, selectedItem))
            {
                SelectedItem = _initializingData.InitialItem;
                _initializingData = null;
                SelectedItem = selectedItem;
            }

            _initializingData = null;
        }

        private class Animator
        {
            private static readonly PropertyPath TranslateXPropertyPath = new PropertyPath(CompositeTransform.TranslateXProperty);
            private static readonly PropertyPath TranslateYPropertyPath = new PropertyPath(CompositeTransform.TranslateYProperty);

            private readonly Storyboard _sbRunning = new Storyboard();

            private readonly DoubleAnimation _daRunning = new DoubleAnimation();

            private readonly Orientation _orientation;

            private CompositeTransform _transform;

            private Action _oneTimeAction;

            public Animator(CompositeTransform compositeTransform, Orientation orientation)
            {
                _transform = compositeTransform;
                _orientation = orientation;

                _sbRunning.Completed += OnCompleted;
                _sbRunning.Children.Add(_daRunning);
                Storyboard.SetTarget(_daRunning, _transform);
                Storyboard.SetTargetProperty(_daRunning, _orientation == Orientation.Horizontal ? TranslateXPropertyPath : TranslateYPropertyPath);
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public double CurrentOffset
            {
                get { return _orientation == Orientation.Horizontal ? _transform.TranslateX : _transform.TranslateY; }
            }

            public Orientation Orientation
            {
                get { return _orientation; }
            }

            public void GoTo(double targetOffset, Duration duration)
            {
                GoTo(targetOffset, duration, null, null);
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void GoTo(double targetOffset, Duration duration, Action completionAction)
            {
                GoTo(targetOffset, duration, null, completionAction);
            }

            [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction)
            {
                GoTo(targetOffset, duration, easingFunction, null);
            }

            public void GoTo(double targetOffset, Duration duration, IEasingFunction easingFunction, Action completionAction)
            {
                _daRunning.To = targetOffset;
                _daRunning.Duration = duration;
                _daRunning.EasingFunction = easingFunction;
                _sbRunning.Begin();
                _sbRunning.SeekAlignedToLastTick(TimeSpan.Zero);
                _oneTimeAction = completionAction;
            }

            private void OnCompleted(object sender, EventArgs e)
            {
                Action action = _oneTimeAction;
                if (action != null && _sbRunning.GetCurrentState() != ClockState.Active)
                {
                    _oneTimeAction = null;
                    action();
                }
            }

            public static bool TryEnsureAnimator(FrameworkElement targetElement, Orientation orientation, ref Animator animator)
            {
                if (animator == null || animator.Orientation != orientation)
                {
                    CompositeTransform transform = Animator.GetCompositeTransform(targetElement);
                    if (transform != null)
                    {
                        animator = new Animator(transform, orientation);
                    }
                    else
                    {
                        animator = null;
                        return false;
                    }
                }
                return true;
            }

            public static CompositeTransform GetCompositeTransform(UIElement container)
            {
                if (container == null)
                {
                    return null;
                }

                return container.RenderTransform as CompositeTransform;
            }
        }

        private class InitializingData
        {
            public int InitialIndex;
            public object InitialItem;
        }

        private enum DragLock
        {
            Unset,
            Free,
            Vertical,
            Horizontal,
        }

        private enum AnimationDirection
        {
            Center,
            Previous,
            Next
        }
    }

    internal static class InternalUtils
    {

        /// <summary>
        /// Retrieves all the visual children of a framework element.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <returns>The visual children of the framework element.</returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject parent)
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int counter = 0; counter < childCount; counter++)
            {
                yield return VisualTreeHelper.GetChild(parent, counter);
            }
        }

        /// <summary>
        /// Retrieves all the logical children of a framework element using a 
        /// breadth-first search.  A visual element is assumed to be a logical 
        /// child of another visual element if they are in the same namescope.
        /// For performance reasons this method manually manages the queue 
        /// instead of using recursion.
        /// </summary>
        /// <param name="parent">The parent framework element.</param>
        /// <returns>The logical children of the framework element.</returns>
        public static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(this FrameworkElement parent)
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            Queue<FrameworkElement> queue =
                new Queue<FrameworkElement>(parent.GetVisualChildren().OfType<FrameworkElement>());

            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                yield return element;

                foreach (FrameworkElement visualChild in element.GetVisualChildren().OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }
        }

        /// <summary>
        /// Gets the ancestors of the element, up to the root, limiting the 
        /// ancestors by FrameworkElement.
        /// </summary>
        /// <param name="node">The element to start from.</param>
        /// <returns>An enumerator of the ancestors.</returns>
        internal static IEnumerable<FrameworkElement> GetVisualAncestors(this FrameworkElement node)
        {
            FrameworkElement parent = node.GetVisualParent();
            while (parent != null)
            {
                yield return parent;
                parent = parent.GetVisualParent();
            }
        }

        /// <summary>
        /// Gets the visual parent of the element.
        /// </summary>
        /// <param name="node">The element to check.</param>
        /// <returns>The visual parent.</returns>
        internal static FrameworkElement GetVisualParent(this FrameworkElement node)
        {
            return VisualTreeHelper.GetParent(node) as FrameworkElement;
        }

        /// <summary>
        /// The first parent of the framework element of the specified type 
        /// that is found while traversing the visual tree upwards.
        /// </summary>
        /// <typeparam name="T">
        /// The element type of the dependency object.
        /// </typeparam>
        /// <param name="element">The dependency object element.</param>
        /// <returns>
        /// The first parent of the framework element of the specified type.
        /// </returns>
        internal static T GetParentByType<T>(this DependencyObject element,string name)
            where T : FrameworkElement
        {
            Debug.Assert(element != null, "The element cannot be null.");

            T result = null;
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            while (parent != null)
            {
                result = parent as T;

                if (result != null)
                {
                    if (result.Name==name)
                    {
                        return result;
                    }
                    
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }


        public static T FindChild<T>(DependencyObject parent, string childName)
   where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }

                    // Need this in case the element we want is nested
                    // in another element of the same type
                    foundChild = FindChild<T>(child, childName);
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }



        internal static bool AreValuesEqual(object o1, object o2)
        {
            if (o1 == o2)
            {
                return true;
            }

            if (o1 == null || o2 == null)
            {
                return false;
            }

            if (o1.GetType().IsValueType || o1.GetType() == typeof(string))
            {
                return object.Equals(o1, o2);
            }

            return object.ReferenceEquals(o1, o2);
        }
    }
}
