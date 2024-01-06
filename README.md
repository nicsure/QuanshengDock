#### **Quansheng Dock**

A combination of firmware and a Windows app that allows remote operation of the Quansheng UV-K5 (and equivalent) hand-held radios via the programming cable.

**Features** 
- Enhanced LCD display cloning. Font and color selection.
- Channel Editor with ability to adjust multiple channels simultaneously.
- Spectrum Analyzer with monitor mode.
- Waterfall display.


**Installation**
: The app is essentially portable and can be placed anywhere. If installing from the release build, just unzip it to a location of your choice and run the "QuanshengDock.exe" inside the folder.
The included firmware file also needs to be flashed to the radio. This can be done with any firmware flashing tool.


**Demonstration**
: You can find a demo video here:


**Hardware Requirements**
: The app will work with just the programming cable but you won't be able to hear the radio's audio as the programming cable cuts out the main speaker. You also won't be able to properly transmit as it cuts out the radio's microphone too. So to transmit voice you'll need an external microphone also.
In order to hear the audio you'll need to make a simple wiring harness. I made mine from 2.5mm/3.5mm stereo Y cable adapters and a 3.5mm stereo patch cable.
- Cut one of the tails of the 2.5mm adapter in half and isolate the inner conductors on both sides, there will be three, An outer shield which is ground, and two wires, one of which carries the audio and the other carries the serial TX.
- Determine which of these is the audio. It is the wire connecting to the tip (end) of the 2.5mm jack plug, you can use a multi-meter in continuity mode to do this.
- Cut back and isolate the audio wire on the tail side (non radio side) as this is not used.
- On the radio side connect a 47k resistor to the audio wire.
- Cut off one of the ends of the patch cable and isolate the inner conductors, there will be three, an outer shield for ground and two inner wires which carry the left and right audio channels.
- Connect the inner wires together and then connect this combined wire to the other side of the resistor.
- Connect both sides of the tail grounds (outer shield) back together and connect those to the patch cable's outer shield.
- Connect the tail's TX data lines back together.
![](./WiringMod.png)
You may be wondering why you can't just use an external speaker microphone, and the reason is because the audio needs to be converted to high-impedance to prevent the audio bleeding over the serial data. If you just plug in a speaker microphone and the programming cable using the Y adapters, when there's any audio from the radio, the serial line gets flooded with garbage data.


**Hardware Configuration**
: As previously mentioned you can just use a programming cable alone without any audio.

If using the wiring harness you should plug the programming cable's 3.5mm jack into any of the 3.5mm Y cable's tails. But the programming cable's 2.5mm jack should be plugged into the unmodified tail of the 2.5mm Y adapter. The external microphone should then be plugged into the remaining tails. This is so that the microphone will not output any audio and corrupt the serial data. 
Plug both Y adapter's jacks into the radio.
