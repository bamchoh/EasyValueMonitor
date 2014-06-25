using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EasyValueMonitor
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ChangeBinding(TextBox sender, BindingMode mode)
        {
            var oldBindingExpression = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            var oldBinding = oldBindingExpression.ParentBinding;

            var new_binding = new Binding
                {
                    Path = oldBinding.Path,
                    UpdateSourceTrigger = oldBinding.UpdateSourceTrigger,
                    Mode = mode
                };
            ((TextBox)sender).SetBinding(TextBox.TextProperty, new_binding);
        }

        private void value_GotFocus(object sender, RoutedEventArgs e)
        {
            ChangeBinding((TextBox)sender, BindingMode.OneTime);
        }

        private void value1_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeBinding((TextBox)sender, BindingMode.TwoWay);
        }

        private void value1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ((MainWindowViewModel)this.DataContext).WriteValue(value1.Text);
                    var element = sender as UIElement;
                    if (element != null)
                        element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    break;
                default:
                    break;
            }
        }
    }

    public class TextBoxBehaviors
    {
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached(
            "IsNumeric", typeof(bool),
            typeof(TextBoxBehaviors),
            new UIPropertyMetadata(false, IsNumericChanged)
            );

		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static void SetIsNumeric(DependencyObject obj, bool value) {
			obj.SetValue(IsNumericProperty, value);
		}

		private static void IsNumericChanged
			(DependencyObject sender, DependencyPropertyChangedEventArgs e) {

			var textBox = sender as TextBox;
			if (textBox == null) return;

			// イベントを登録・削除 
			textBox.KeyDown -= OnKeyDown;
			textBox.TextChanged -= OnTextChanged;
			var newValue = (bool)e.NewValue;
			if (newValue) {
				textBox.KeyDown += OnKeyDown;
				textBox.TextChanged += OnTextChanged;
			}
		}

		static void OnKeyDown(object sender, KeyEventArgs e) {
			var textBox = sender as TextBox;
			if (textBox == null) return;

			if ((Key.D0 <= e.Key && e.Key <= Key.D9) ||
				(Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) ||
				(Key.Delete == e.Key) || (Key.Back == e.Key) || (Key.Tab == e.Key)) {
				e.Handled = false;
			} else {
				e.Handled = true;
			}
		}

		private static void OnTextChanged(object sender, TextChangedEventArgs e) {
			var textBox = sender as TextBox;
			if (textBox == null) return;

			if (string.IsNullOrEmpty(textBox.Text)) {
				textBox.Text = "0";
			}
		}
    }
}
