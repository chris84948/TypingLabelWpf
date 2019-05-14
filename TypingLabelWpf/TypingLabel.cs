using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace TypingLabelWpf
{
    public class TypingLabel : Control
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnTextChanged)));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TimeBetweenInMsProperty =
            DependencyProperty.Register("TimeBetweenInMs",
                                        typeof(double),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(50.0, new PropertyChangedCallback(OnTimeBetweenInMsChanged)));
        public double TimeBetweenInMs
        {
            get { return (double)GetValue(TimeBetweenInMsProperty); }
            set { SetValue(TimeBetweenInMsProperty, value); }
        }

        public static readonly DependencyProperty StartDelayInMsProperty =
            DependencyProperty.Register("StartDelayInMs",
                                        typeof(double),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(0.0));
        public double StartDelayInMs
        {
            get { return (double)GetValue(StartDelayInMsProperty); }
            set { SetValue(StartDelayInMsProperty, value); }
        }

        public static readonly DependencyProperty EndDelayInMsProperty =
            DependencyProperty.Register("EndDelayInMs",
                                        typeof(double),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(5000.0));
        public double EndDelayInMs
        {
            get { return (double)GetValue(EndDelayInMsProperty); }
            set { SetValue(EndDelayInMsProperty, value); }
        }

        public static readonly DependencyProperty RepeatBehaviorProperty =
            DependencyProperty.Register("RepeatBehavior",
                                        typeof(RepeatBehavior),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(RepeatBehavior.Once));
        public RepeatBehavior RepeatBehavior
        {
            get { return (RepeatBehavior)GetValue(RepeatBehaviorProperty); }
            set { SetValue(RepeatBehaviorProperty, value); }
        }

        public static readonly DependencyProperty TypingStyleProperty =
            DependencyProperty.Register("TypingStyle",
                                        typeof(TypingStyle),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(TypingStyle.Letter, new PropertyChangedCallback(OnTypingStyleChanged)));
        public TypingStyle TypingStyle
        {
            get { return (TypingStyle)GetValue(TypingStyleProperty); }
            set { SetValue(TypingStyleProperty, value); }
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment",
                                        typeof(TextAlignment),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(TextAlignment.Left));
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping",
                                        typeof(TextWrapping),
                                        typeof(TypingLabel),
                                        new FrameworkPropertyMetadata(TextWrapping.Wrap));
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        private static readonly DependencyPropertyKey InternalTextPropertyKey =
            DependencyProperty.RegisterReadOnly("InternalText",
                                                typeof(string),
                                                typeof(TypingLabel),
                                                new PropertyMetadata(""));
        public static readonly DependencyProperty InternalTextProperty = InternalTextPropertyKey.DependencyProperty;
        public string InternalText
        {
            get { return (string)GetValue(InternalTextProperty); }
            private set { SetValue(InternalTextPropertyKey, value); }
        }

        static TypingLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TypingLabel), new FrameworkPropertyMetadata(typeof(TypingLabel)));
        }

        private DispatcherTimer _startDelayTimer, _endDelayTimer, _typeTimer, _blinkTimer;
        private int _index = 0, _numTicksInWord = 0;
        private string[] _words;
        private bool _showCarat = true;
        private Run _cursor, _cursorTransparent;

        public TypingLabel()
        {
            Unloaded += TypingLabel_Unloaded;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _cursor = GetTemplateChild("PART_Cursor") as Run;
            _cursorTransparent = GetTemplateChild("PART_CursorTransparent") as Run;

            _startDelayTimer = new DispatcherTimer();
            _startDelayTimer.Interval = TimeSpan.FromMilliseconds(StartDelayInMs);
            _startDelayTimer.Tick += _startDelayTimer_Tick;

            _endDelayTimer = new DispatcherTimer();
            _endDelayTimer.Interval = TimeSpan.FromMilliseconds(EndDelayInMs);
            _endDelayTimer.Tick += _endDelayTimer_Tick;

            _typeTimer = new DispatcherTimer();
            _typeTimer.Interval = TimeSpan.FromMilliseconds(TimeBetweenInMs);
            _typeTimer.Tick += _typeTimer_Tick;

            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromMilliseconds(500);
            _blinkTimer.Tick += _blinkTimer_Tick;
            _blinkTimer.Start();

            _startDelayTimer?.Start();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TypingLabel)?._startDelayTimer?.Start();
        }

        private static void OnTimeBetweenInMsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TypingLabel label)
            {
                label._typeTimer.Interval = TimeSpan.FromMilliseconds((double)e.NewValue);
                label._startDelayTimer?.Start();
            }
        }

        private static void OnTypingStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TypingLabel)?.ResetAndStartTyping();
        }

        private void ResetAndStartTyping()
        {
            _index = 0;
            _typeTimer.Start();
            _blinkTimer.Stop();
            _showCarat = true;

            InternalText = "";

            if (_cursor != null)
                _cursor.Text = "|";

            if (_cursorTransparent != null)
                _cursorTransparent.Text = "";

            if (TypingStyle == TypingStyle.Word)
                _words = Text.Split(' ');
        }

        private void _startDelayTimer_Tick(object sender, EventArgs e)
        {
            _startDelayTimer.Stop();
            ResetAndStartTyping();
        }

        private void _endDelayTimer_Tick(object sender, EventArgs e)
        {
            _endDelayTimer.Stop();

            if (RepeatBehavior == RepeatBehavior.Forever)
            {
                InternalText = "";
                _startDelayTimer.Start();
            }
        }

        private void _typeTimer_Tick(object sender, EventArgs e)
        {
            if (_numTicksInWord > 0)
            {
                _numTicksInWord--;
                return;
            }

            if (TypingStyle == TypingStyle.Letter)
            {
                InternalText += Text[_index];
                _index++;

                if (_index >= Text.Length)
                {
                    _typeTimer.Stop();
                    _blinkTimer.Start();
                    _endDelayTimer.Start();
                }
            }
            else
            {
                InternalText += (InternalText.Length > 0 ? " " : "") + _words[_index];
                _index++;

                if (_index >= _words.Length)
                {
                    _typeTimer.Stop();
                    _blinkTimer.Start();
                    _endDelayTimer.Start();
                }
                else
                {
                    // Counts the ticks before writing next word - makes long words take longer
                    _numTicksInWord = _words[_index].Length + 1;
                }
            }
        }

        private void _blinkTimer_Tick(object sender, EventArgs e)
        {
            _showCarat = !_showCarat;

            _cursor.Text = _showCarat ? "|" : "";
            _cursorTransparent.Text = _showCarat ? "" : "|";
        }

        private void TypingLabel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_typeTimer == null)
                return;

            _typeTimer.Stop();
            _typeTimer = null;

            _blinkTimer.Stop();
            _blinkTimer = null;
        }
    }
}