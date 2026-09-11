using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace VideoPlayerWpf
{
  public partial class MainWindow : Window
  {
    DispatcherTimer timerPosition = new();
    OpenFileDialog openFileDialog = new();
    bool isPlaying, isDraggingSeek;
    TimeSpan duration;


    public MainWindow()
    {
      InitializeComponent();

      timerPosition.Interval = TimeSpan.FromMilliseconds(250);
      timerPosition.Tick += TimerPosition_Tick;
      timerPosition.Start();

      sliderVolume.Value = 40;
    }

    void TimerPosition_Tick(object? sender, EventArgs e)
    {
      if (isDraggingSeek || player.Source == null) return;
      UpdateTrackBarSeekPosition();
    }

    void UpdateTrackBarSeekPosition()
    {
      var position = player.Position;

      int seconds = (int)position.TotalSeconds;
      if (seconds <= sliderSeek.Maximum)
        sliderSeek.Value = seconds;

      labelTime.Content = $"{FormatTime(position)} / {FormatTime(duration)}";
    }

    string FormatTime(TimeSpan time) => time.ToString(@"h\:mm\:ss");

    void ButtonSelectVideo_Click(object sender, RoutedEventArgs e)
    {
      if (openFileDialog.ShowDialog() != true) return;

      player.Source = new Uri(openFileDialog.FileName);
      labelNowPlaying.Content = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
      UpdatePlayPauseState(isNowPlaying: true);
    }

    void ButtonSkipBack_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan newSeekPosition = TimeSpan.FromSeconds(sliderSeek.Value - 300);
      if (newSeekPosition.TotalSeconds < 0)
        newSeekPosition = TimeSpan.Zero;

      player.Position = newSeekPosition;
      UpdateTrackBarSeekPosition();
    }

    void ButtonPlayPause_Click(object sender, RoutedEventArgs e)
    {
      if (player.Source == null) return;
      UpdatePlayPauseState(!isPlaying);
    }

    void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
      player.Stop();
      player.Position = TimeSpan.Zero;
      UpdatePlayPauseState(isNowPlaying: false);
      UpdateTrackBarSeekPosition();
    }

    void ButtonSkipForward_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan newSeekPosition = TimeSpan.FromSeconds(sliderSeek.Value + 300);
      if (newSeekPosition.TotalSeconds > sliderSeek.Maximum) return;

      player.Position = newSeekPosition;
      UpdateTrackBarSeekPosition();
    }

    void Player_MediaOpened(object sender, RoutedEventArgs e)
    {
      if (!player.NaturalDuration.HasTimeSpan) return;

      duration = player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.Zero;
      sliderSeek.Maximum = Math.Max(1, (int)duration.TotalSeconds);
      UpdateTrackBarSeekPosition();
    }

    void Player_MediaEnded(object sender, RoutedEventArgs e)
    {
      UpdatePlayPauseState(isNowPlaying: false);
      player.Stop();
      UpdateTrackBarSeekPosition();
    }

    void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
      timerPosition.Stop();
      MessageBox.Show($"Playback failed:\n{e.ErrorException.Message}",
                      "Playback error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      player.Volume = sliderVolume.Value / 100.0;
    }

    void SliderSeek_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (player.Source == null) return;

      if (IsThumb(e.OriginalSource as DependencyObject))
      {
        isDraggingSeek = true;  // user is dragging the slider thumb
        return;
      }

      // jump straight to the clicked position
      Point clickPoint = e.GetPosition(sliderSeek);
      double ratio = clickPoint.X / sliderSeek.ActualWidth;
      ratio = Math.Clamp(ratio, 0, 1);
      double newValue = sliderSeek.Minimum + ratio * (sliderSeek.Maximum - sliderSeek.Minimum);
      sliderSeek.Value = newValue;

      player.Position = TimeSpan.FromSeconds(newValue);
      UpdateTrackBarSeekPosition();
      e.Handled = true;
    }

    bool IsThumb(DependencyObject? source)
    {
      while (source != null)
      {
        if (source is Thumb) return true;
        source = VisualTreeHelper.GetParent(source);
      }
      return false;
    }

    void SliderSeek_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      if (player.Source == null) return;
      if (!isDraggingSeek) return;

      isDraggingSeek = false;

      // jump to where thumb was dragged
      player.Position = TimeSpan.FromSeconds(sliderSeek.Value); 
      UpdateTrackBarSeekPosition();
    }

    void sliderSeek_MouseMove(object sender, MouseEventArgs e)
    {
      if (player.Source == null) return;
      if (!isDraggingSeek) return;

      var position = TimeSpan.FromSeconds(sliderSeek.Value);
      labelTime.Content = $"{FormatTime(position)} / {FormatTime(duration)}";
    }

    void UpdatePlayPauseState(bool isNowPlaying)
    {
      isPlaying = isNowPlaying;

      if (isNowPlaying)
      {
        player.Play();
        isPlaying = true;
        buttonPlayPause.Content = "⏸";
        timerPosition.Start();
      }
      else
      {
        player.Pause();
        isPlaying = false;
        buttonPlayPause.Content = "▶";
        timerPosition.Stop();
      }
    }
  }
}