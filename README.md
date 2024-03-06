# **Quansheng Dock** V 0.32.20q

# Requires firmware 0.32.8x - No kidding.
https://github.com/nicsure/quansheng-dock-fw/releases/tag/0.32.8x

  There are some considerable configuration changes and it may break previous configs. I'm sorry if this happens but it's unlikely I caught every potential conflict. Clearing the config may help any issues, in your Documents folder, look for QuanshengDock folder and either delete it, or rename it to clear the previous config.  
  
  You may find that presets/channels in the XVFO now have a different mode than before. Again sorry about this, but was unavoidable.




  
# A WPF Windows app that allows remote operation of the Quansheng UV-K5 (and equivalent) hand-held radios via the programming cable.
- If upgrading from a version before 0.27.3q the new version will likely have HUGE text on the LCD screen. This is because I had to change the way the "F Stretch" feature worked. If this happens open up your settings and change "F Stretch" to a value around 0.20

**Features**
- Enhanced preset/channel scanner.
- Hardware level VFO (experimental)
- Enhanced LCD display cloning. Font and color selection.
- Channel Editor with ability to adjust multiple channels simultaneously.
- RepeaterBook Integration.
- Spectrum Analyzer with monitor mode.
- Waterfall display.
- Audio passthrough.

**Recent Changes**
- Further changes will be listed in the release notes instead of here
- Messenger
- XVFO CW Mode
- CHIRP Fix
- Preset selector wheel
- Watch and Respond feature
- Mutiple VFOs A, B, C and D added
- Mouse wheel changes XVFO freq by set step when pointer is over jog wheel.
- Individual digit adjustment with mouse wheel
- XVFO Scanner facelift
- XVFO preset scanner (very early design, functional but ugly. Just like me)
- Fixed two erroneous DCS codes
- Added Ultra Wide and Ultra Low bandwidth settings, not sure how useful they'll be though. (XVFO)
- Fixed bug in channel editor preventing entry of negative offset frequency.
- Auto Squelch (XVFO).
- Channel import to XVFO Presets.
- Mic Gain (XVFO).
- RX CTCSS/DCS Implemented (XVFO).
- XVFO TX implemented as well as other features.

**TO DO**
- Plenty

**Download**
: A pre-compiled release is available here
https://github.com/nicsure/QuanshengDock/releases/tag/0.32.20q  
: You also need .NET 6 runtime  
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.418-windows-x64-installer


**Installation**
: The app is essentially portable and can be placed anywhere. If installing from the release build, just unzip it to a location of your choice and run the "QuanshengDock.exe" inside the folder.
You will also need to program the radio with the accompanying firmware found here : https://github.com/nicsure/quansheng-dock-fw


**Demonstration**
: You can find a demo video here: https://www.youtube.com/watch?v=UwTz5wricmY

# Hardware Requirements
The app will work with just the programming cable but you won't be able to hear the radio's audio as the programming cable cuts out the main speaker. You also won't be able to properly transmit as it cuts out the radio's microphone too. So to get audio in and out of the radio you'll need some extra hardware.  

**AIOC (Recommended)**  
I STRONGLY recommend using the excellent AOIC cable with this software.
- You can find the AIOC GitHub page here https://github.com/skuep/AIOC  
- A guide for configuring the AIOC with Quansheng Dock can be found on my YouTube channel here https://www.youtube.com/watch?v=IrnzWVdOlu8

**Makeshift Audio Interface (Not Recommended)**  
In order to hear the audio you can make a simple wiring harness. I made mine from 2.5mm/3.5mm stereo Y cable adapters, a 47k resistor and a 3.5mm stereo patch cable.
- Cut one of the tails of the 2.5mm adapter in half and isolate the inner conductors on both sides, there will be three, An outer shield which is ground, and two wires, one of which carries the audio and the other carries the serial TX.
- Determine which of these is the audio. It is the wire connecting to the tip (end) of the 2.5mm jack plug, you can use a multi-meter in continuity mode to do this.
- Cut back and isolate the audio wire on the tail side (non radio side) as this is not used.
- On the radio side connect a 47k resistor to the audio wire.
- Cut off one of the ends of the patch cable and isolate the inner conductors, there will be three, an outer shield for ground and two inner wires which carry the left and right audio channels.
- Connect the inner wires together and then connect this combined wire to the other side of the resistor.
- Connect both sides of the tail grounds (outer shield) back together and connect those to the patch cable's outer shield.
- Connect the tail's TX data lines back together.
![](./WiringMod2.png)
![](./WiringSchematic.png)
You may be wondering why you can't just use an external speaker, the reason is because the audio needs to be converted to high-impedance to prevent the audio bleeding over the serial data. If you just connect a low impedance speaker (like in a speaker microphone) and the programming cable using the Y adapters, when there's any audio from the radio, the serial line gets flooded with garbage data.


**Hardware Configuration**
: As previously mentioned you can just use a programming cable alone without any audio, but if you're using the wiring harness
- plug the programming cable's 3.5mm jack into either of the 3.5mm Y cable's tails.
- The programming cable's 2.5mm jack should be plugged into the **UNMODIFIED** tail of the 2.5mm Y adapter.
- The microphone's 2.5mm jack should be plugged into the **MODIFIED** tail.
- Plug the microphone's 3.5mm jack into the remaining tail.
- Plug both Y adapter's jacks into the radio.
- Connect the 3.5mm patch lead to your PC's line input or you may use any device that can accept audio line input.
![](./WiringOverview2.png)  

 
# **Manual**
Please take a look at the [Online Manual (see Wiki)](https://github.com/nicsure/QuanshengDock/wiki) for instructions, tips and more
* [First impression...](https://github.com/nicsure/QuanshengDock/wiki)
* [Basic idea and operation](https://github.com/nicsure/QuanshengDock/wiki/10-%E2%80%90-Basic-idea-and-operation)
* [The radio firmware](https://github.com/nicsure/QuanshengDock/wiki/20-%E2%80%90-The-radio-firmware)
* [Connection between radio and PC](https://github.com/nicsure/QuanshengDock/wiki/30-%E2%80%90-Connection-between-radio-and-PC)
* [Q‐Dock program on PC](https://github.com/nicsure/QuanshengDock/wiki/40-%E2%80%90-Q%E2%80%90Dock-program-on-PC)
* [Specific Functions explained](https://github.com/nicsure/QuanshengDock/wiki/50-%E2%80%90-Specific-Functions-explained)
* [Integrating QD with other programs](https://github.com/nicsure/QuanshengDock/wiki/60-%E2%80%90-Integrating-QD-with-other-programs)
* [Tips & Tricks and Known Issues](https://github.com/nicsure/QuanshengDock/wiki/70-%E2%80%90-Tips-&-Tricks-and-Known-Issues)
* [Reviews on Youtube](https://github.com/nicsure/QuanshengDock/wiki/80-%E2%80%90-Reviews-on-Youtube)


# **Support**  
I have been asked several times if there is any way to support me financially. Yes, you can do so by sending BitCoin to the following address.  
Please DO NOT send what you cannot afford. I of all people understand the cost of living under the current economical disaster and I'm struggling to live just like everyone else. So please, only donate if you can afford to do so.  
  
Bitcoin: 3FJEy42F6WUnjtxWwgSYxDHmjVFooLKCDH  
<img src='./btc qr.png' width='100' />
