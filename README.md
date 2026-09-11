# Video Player WPF
## C# with .NET 10 and WPF

A super-simple desktop app for people getting started with video rendering and Windows Presentation Foundation.

<img width="793" height="557" alt="image" src="https://github.com/user-attachments/assets/0fa8203a-43cd-42f0-966e-b6db5168e3ff" />

*Figure 1 - VideoPlayerWpf in action.*

The app can handle the most common video file types, including:

* .mp4 (both H.264 and H.265)
* .mov (Apple QuickTime)
* .wmv (Windows Media Video)

With VideoPlayerWpf, you first open a video file, then use the various control buttons to play it.
The heavy lifting is handled by the MediaPlayer control that is part of Windows Presentation Foundation (WPF). 

## The UI Layout

The UI is laid out using a series of docked panels managed by the container DockPanel. For the panels we use Border controls, as shown in the following figure.

<img width="728" height="557" alt="image" src="https://github.com/user-attachments/assets/8ee36726-38ea-48d8-b371-3ab68c530767" />

*Figure 2 - Using DockPanel and Border controls to layout the user interface.*

When laying out docked controls, their order (back to front) is important. The first control added in the Visual Studio Visual Designer is the backmost, and appears first in the list of DockPanel children.
Additional controls appear in front of controls already added. With top-docked controls, the first one goes to the top edge, the second one docks right under it and so on.
Conversely, for bottom-docked controls the first one goes to the bottom, the second one docks right above it, and so on.
When DockPanel has LastChildFill=True, the last control added will fill any available space left over. We use the video container as the last child. 
The following listing shows the XAML for the layout.

```xml
<DockPanel x:Name="MainContainer" LastChildFill="True" Background="Black">
  <Border x:Name="borderNowPlaying" DockPanel.Dock="Top" Height="30" >...</Border>
  <Border x:Name="borderPlaybackControls" DockPanel.Dock="Bottom" Height="64">...</Border>
  <Border x:Name="borderSlider" DockPanel.Dock="Bottom" Height="40">...</Border>
  <Border x:Name="borderVideo" Background="Black" ><MediaElement x:Name="player" .../></Border>
</DockPanel>
```
*Listing 1 -The XAML used to achieve the UI layout.*


The MediaElement player is contained in the last Border element, so it resizes automatically if we resize the app. 
It is capable of handling both audio and video files. With audio files, the video remains black.

## Controlling Playback

MediaElement exposes all the required playback and control methods to load, play, pause, stop and seek. Controlling the player is 
almost trivial. The following listing shows the main code.

```csharp
    void ButtonSelectVideo_Click(object sender, RoutedEventArgs e)
    {
      if (openFileDialog.ShowDialog() != true) return;  // ask user to select a video file

      player.Source = new Uri(openFileDialog.FileName);  // load video into player
      //...
    }

    void ButtonSkipBack_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan newSeekPosition = TimeSpan.FromSeconds(sliderSeek.Value - 300);
      //...
      player.Position = newSeekPosition;  // jump back 5 minutes
      //...
    }

    void ButtonPlayPause_Click(object sender, RoutedEventArgs e)
    {
      //...
      player.Pause();  // pause video playback
    }

    void ButtonStop_Click(object sender, RoutedEventArgs e)
    {
      //...
      player.Stop();  // stop video playback
    }

    void ButtonSkipForward_Click(object sender, RoutedEventArgs e)
    {
      TimeSpan newSeekPosition = TimeSpan.FromSeconds(sliderSeek.Value + 300);
      //...
      player.Position = newSeekPosition;  // jump forward 5 minutes
    }
```
*Listing 2 -The main code controlling the MediaElement player.*

The code I left out is related to updating the video time and the seek track.

## Player Events

The MediaElement player fires three events of interest to us:

1. *MediaOpened.* Fired once the player loads a new file.
2. *MediaEnded.* Fired when the player reaches the end of a video.
3. *MediaFailed.* Fired if the player encounters problems with the video.

The following listing shows the XAML related to the events.

```xml
<Border x:Name="borderVideo" ... >
  <MediaElement x:Name="player"
                //...
                MediaOpened="Player_MediaOpened" 
                MediaEnded="Player_MediaEnded" 
                MediaFailed="Player_MediaFailed"/>
</Border>
```

*Listing 3 -The XAML code declaring the player and binding its events to event handlers.*

The following listing shows the salient code of the event handlers.

```csharp
  void Player_MediaOpened(object sender, RoutedEventArgs e)
  {
    //...
    // set the video length on the seek slider
    var duration = player.NaturalDuration.TimeSpan;
    sliderSeek.Maximum = (int)duration.TotalSeconds;
    //...
  }
  
  void Player_MediaEnded(object sender, RoutedEventArgs e)
  {
   //...
    player.Stop();
    //...
  }
  
  void Player_MediaFailed(object sender, ExceptionRoutedEventArgs e)
  {
    timerPosition.Stop();  // used to update the video time
    MessageBox.Show(e.ErrorException.Message, "Playback error");  // show error message
  }
```

*Listing 4 -The player event handlers.*

## Using Font Symbols for the video control buttons

A note regarding the symbols on the play buttons. Once upon a time we would have needed to import them as bitmaps into the project and add them as embedded resources.
All that complication is avoided by using the symbols as font characters. Several fonts include media player symbols. You can lookup symbols on the web or simply use the
Character Map app, which is built into Windows itself. Below are the symbols we need, with their Unicode values.

⏪  U+23EA: Skip back

▶   U+25B6: Play

⏸  U+23F8: Pause

⏹  U+23F9: Stop

⏩  U+23E9: Skip forward

The figure below shows the use of Windows Character Map to do the looking up.

<img width="476" height="507" alt="image" src="https://github.com/user-attachments/assets/0493fe9c-995a-4884-a421-39c1af5ebbef" />

*Figure 4 - Using the Windows Character Map to find symbols used in VideoPlayer.*

## Closing Notes

* *Codecs.* The WPF MediaPlayer is shipped with the most common codecs included. Additional codecs can be downloaded from the Windows Media Foundation.
* *Performance.* The player is designed for ease-of-use, not high-performance. It handles video decoding in software rather than using hardware acceleration.
* *Audio Support.* The player supports audio. If you open an audio file in VideoPlayer, the video panel will just remain black.
