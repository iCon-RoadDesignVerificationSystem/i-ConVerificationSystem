using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace i_ConVerificationSystem.Forms.Base
{
    /// <summary>
    /// キーバインド付きMenuItem
    /// </summary>
    class MenuItemKeyBinded : MenuItem
    {
        /// <summary>
        /// KeyBinding依存関係プロパティ
        /// </summary>
        public KeyBinding KeyBind
        {
            get { return (KeyBinding)GetValue(KeyBindProperty); }
            set { SetValue(KeyBindProperty, value); }
        }
        public static readonly DependencyProperty KeyBindProperty =
            DependencyProperty.Register(nameof(KeyBind), typeof(KeyBinding), typeof(MenuItemKeyBinded),
                new PropertyMetadata(new KeyBinding(),
                    //KeyBindingが指定されたときに呼ばれるコールバック
                    KeyBindPropertyChanged));

        private static void KeyBindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menuItemKB = d as MenuItemKeyBinded;
            var kb = e.NewValue as KeyBinding;
            //KeyBindingに結び付けられたコマンドをこのMenuItemのCommandに反映
            menuItemKB.Command = kb.Command;
            //KeyBindingのローカライズされた文字列("Ctrl"など)をこのMenuItemのInputGestureTextに反映
            menuItemKB.InputGestureText = (kb.Gesture as KeyGesture).GetDisplayStringForCulture(CultureInfo.CurrentCulture);
        }
    }
}
