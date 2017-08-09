using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Karpach.DebugAttachManager.Helpers;
using Button = System.Windows.Controls.Button;
using MenuItem = System.Windows.Controls.MenuItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Karpach.DebugAttachManager.Controls
{
    /// <summary>
    /// Represents a combination of a standard button on the left and a drop-down button on the right.
    /// </summary>
    [TemplatePartAttribute(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePartAttribute(Name = "PART_Button", Type = typeof(Button))]
    public class SplitButton : MenuItem
    {
        private Button _splitButtonHeaderSite;        

        /// <summary>
        /// Identifies the CornerRadius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty;

        private static readonly RoutedEvent ButtonClickEvent;

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));

            CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(typeof(SplitButton));

            IsSubmenuOpenProperty.OverrideMetadata(typeof(SplitButton),
                new FrameworkPropertyMetadata(
                    BooleanBoxes.FalseBox,
                    OnIsSubmenuOpenChanged,
                    CoerceIsSubmenuOpen));

            ButtonClickEvent = EventManager.RegisterRoutedEvent("ButtonClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SplitButton));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
            KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(KeyboardNavigationMode.None));


            EventManager.RegisterClassHandler(typeof(SplitButton), ClickEvent, new RoutedEventHandler(OnMenuItemClick));            
            EventManager.RegisterClassHandler(typeof(SplitButton), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnMouseButtonDown), true);
            EventManager.RegisterClassHandler(typeof(SplitButton), Mouse.MouseLeaveEvent, new System.Windows.Input.MouseEventHandler(OnMouseLeave), true);
        }

        /// <summary>
        /// Gets or sets a value that represents the degree to which the corners of a <see cref="SplitButton"/> are rounded.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Occurs when the button portion of a <see cref="SplitButton"/> is clicked.
        /// </summary>
        public event RoutedEventHandler ButtonClick
        {
            add => AddHandler(ButtonClickEvent, value);
            remove => RemoveHandler(ButtonClickEvent, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _splitButtonHeaderSite = GetTemplateChild("PART_Button") as Button;
            if (_splitButtonHeaderSite != null)
            {
                _splitButtonHeaderSite.Click += OnHeaderButtonClick;
            }                        
        }

        private void OnHeaderButtonClick(Object sender, RoutedEventArgs e)
        {
            OnButtonClick();
        }

        protected virtual void OnButtonClick()
        {
            RaiseEvent(new RoutedEventArgs(ButtonClickEvent, this));
        }

        private static void OnIsSubmenuOpenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitButton = sender as SplitButton;
            if ((Boolean)e.NewValue)
            {
                if (!Equals(Mouse.Captured, splitButton))
                {
                    Mouse.Capture(splitButton, CaptureMode.SubTree);
                }
            }
            else
            {
                if (Equals(Mouse.Captured, splitButton))
                {
                    Mouse.Capture(null);
                }

                if (splitButton != null && splitButton.IsKeyboardFocused)
                {
                    splitButton.Focus();
                }
            }
        }

        /// <summary>
        /// Set the IsSubmenuOpen property value at the right time.
        /// </summary>
        private static Object CoerceIsSubmenuOpen(DependencyObject element, Object value)
        {
            SplitButton splitButton = element as SplitButton;
            if ((Boolean)value)
            {
                if (splitButton != null && !splitButton.IsLoaded)
                {
                    splitButton.Loaded += delegate
                    {
                        splitButton.CoerceValue(IsSubmenuOpenProperty);
                    };

                    return BooleanBoxes.FalseBox;
                }
            }

            return splitButton != null && (Boolean)value && splitButton.HasItems;
        }

        private static void OnMenuItemClick(Object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = e.OriginalSource as MenuItem;

            // To make the ButtonClickEvent get fired as we expected, you should mark the ClickEvent 
            // as handled to prevent the event from poping up to the button portion of the SplitButton.
            if (menuItem != null && !(menuItem.Parent is MenuItem))
            {
                e.Handled = true;                
            }
            var parent = menuItem?.Parent as SplitButton;
            parent?.CloseSubmenu();
        }        

        private static void OnMouseButtonDown(Object sender, MouseButtonEventArgs e)
        {
            SplitButton splitButton = sender as SplitButton;
            if (splitButton != null && !splitButton.IsKeyboardFocusWithin)
            {
                splitButton.Focus();
                return;
            }

            if (Equals(Mouse.Captured, splitButton) && Equals(e.OriginalSource, splitButton))
            {
                splitButton?.CloseSubmenu();             
            }            
        }

        private static void OnMouseLeave(Object sender,  MouseEventArgs e)
        {
            SplitButton splitButton = sender as SplitButton;
            splitButton?.CloseSubmenu();
        }

        private void CloseSubmenu()
        {
            if (IsSubmenuOpen)
            {
                ClearValue(IsSubmenuOpenProperty);
                if (IsSubmenuOpen)
                {
                    IsSubmenuOpen = false;
                }
            }
        }
    }
}