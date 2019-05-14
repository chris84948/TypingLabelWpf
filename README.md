# WPF Typing Label Control

If your application is needing a little sparkle, this is the project for you!

This typing label control emulates a person typing the letters or words supplied. It works really well to catch the eye of the user when entering a new screen.

![Example](https://raw.githubusercontent.com/chris84948/TypingLabelWpf/master/example.gif)

Full example is supplied in the repo, but here's the basics.

    <tl:TypingLabel EndDelayInMs="2000"
                    StartDelayInMs="3000"
                    FontFamily="Consolas"
                    FontSize="16"
                    RepeatBehavior="Forever"
                    Text="The quick brown fox jumps over the lazy dog."
                    TypingStyle="Letter" />
                    
The label acts like a regular label, so font size, font family, foreground etc all work. There are a couple of extra properties available though.

- TypingStyle, Word or Letter (Letter is default)
- RepeatBehavior, Once or Forever (Once is default)
- StartDelayInMs (0ms is default)
- EndDelayInMs (5000ms is default)

[Nuget Package available here](https://www.nuget.org/packages/TypingLabelWpf/)
