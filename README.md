# Guitar-Hero-Clone
(2024) A guitar hero clone that allows playing of community-made song charts, taking input from a keyboard or my Arduino-modified guitar controller

My ps3 Guitar Hero controller stopped working so I used an arduino UNO to read voltages directly from the buttons, and made this guitar hero clone to take those inputs and allow me to play community-charted songs.

(Note miss sounds used in the project are not provided as they are copyrighted)

## Setup:
Custom songs must:
- include notes.chart (.mid not supported)
- include song.wav (.opus not supported)
- not contain * in folder name
- be placed in Songs folder, unzipped

offset can be changed in the chart file to sync the audio (-/+)

## Technologies/Techniques Used:
- Parsing of .chart file format to populate fret board with notes
- Converting analogue voltage values to digital inputs in c++ on arduino
- Input device communication over COM port

## Features:
- Up to 5 frets, star power, combo multiplier, star/tap/open/sustain notes, and tempo changes
- Progress bar and highscore visible during song
- Input via keyboard or arduino-modified guitar via COM port
- Detailed UI with difficulty and audio settings
- Automatic highscore saving and loading
- Support for custom songs in popular .chart format
- Music video / background image support
- Audio previews when choosing songs
- Input tester to visualise button presses
- Option for audible miss notes

## Gallery:
<img src="https://github.com/user-attachments/assets/ba565200-d6ac-4684-ba31-2c07680e7d63" width="311" height="174" style="display:none">
<img src="https://github.com/user-attachments/assets/9ae43592-3073-4c4f-870e-c3527d849b92" width="311" height="174" style="display:none">
<img src="https://github.com/user-attachments/assets/a97393f5-3cb3-4337-b5d2-1504d8343a96" width="311" height="174" style="display:none">
<img src="https://github.com/user-attachments/assets/4f8a636f-0f6c-4da5-9355-18db2c5f00b0" width="311" height="174" style="display:none">

## Acknowledgements:
The following assets were used in this project and were not created by myself:
- Dyadica - Unity Serial Port Script
https://github.com/dyadica/Unity_SerialPort

- Marc Khouri - I2C communication with guitar neck
https://marc.khouri.ca/posts/2022/GH5-neck-comms.html
