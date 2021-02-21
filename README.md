# SPOT
Sparta's Picture Optimizing Tool for the Commodore 64
-----------------------------------------------------

SPOT is a small Windows tool that converts .png, .bmp, and .kla images into C64 file formats optimized for better compression. The following output file formats can be selected: koala (.kla), bitmap (.map), screen RAM (.scr), color RAM (.col), compressed color RAM (.ccr)*, and optimized bitmap (.obm)**. Additionally, SPOT can also create .png, .bmp, and .jpg files.


Usage:
------
SPOT can be used both as a tool window and from command line.


Tool window functions:
----------------------
Settings: Select output format(s). Autosave to SPOT subfolder - if enabled, SPOT creates a SPOT\filename subfolder and all selected files are output there. If Autosave is disabled then the user will be able to select an output folder and base file name during conversion. File extension in the save dialogue window is ignored and will be determined by the selected output formats.
View Koala: Only allows a preview of a koala file. Good for before and after comparison. Press 1-5 to see color distribution.
Convert & Save: SPOT loads, converts and optimizes a supported image format and saves it in the output folder. The background color will be added to the output file name in the case of C64 formats. Pressing the left mouse button over the image window (post conversion) allows a comparison of original and converted images. Differences should be only visible if the original image uses a non-default C64 palette.


Command line usage:
-------------------
SPOT uses the following format in command line:

spot infile kmsc2opbj outfile

infile: an input image file to be optimized/converted, only .png, .bmp, and .kla are accepted
kmsc2opbj: output formats, select as many as you want in any order:
	k - .kla
	m - .map
	s - .scr
	c - .col
	2 - .ccr
	o - .obm
	p - .png
	b - .bmp
	j - .jpg
outfile - the output folder and file name, extension is ignored

The last two arguments can be omitted, but each one is dependent on the one on its left. I.e. if one omits the output format then the outfile argument must be omitted too. If outfile is omitted only, SPOT will use the SPOT\filename folder and the input file's name. If output formats are omitted too than SPOT will use the formats selected in the Settings window.


Examples for command line usage:
--------------------------------

spot test.png kmsc testout\testconv
converts test.png to .kla, .map, .scr, and .col formats and saves the output to the testout folder using testconv as base filename

spot test.png kmsc
converts test.png to .kla, .map, .scr, and .col formats and saves the output to the SPOT\test folder using test as base filename

spot test.png
converts test.png to the formats previously selected in the Settings window and saves the output to the SPOT\test subfolder using test as base filename


Notes:
------
SPOT checks all possible background colors and generates multiple outputs if possible (e.g. for images using less than 16 colors).
SPOT recognizes several C64 palettes. If a palette match is not found then it attempts to convert colors to a standard C64 palette.
SPOT can handle non-standard image sizes (such as the vertical bitmap in Memento Mori and the diagonal bitmap in the Christmas Megademo). When a koala file is created from a non-standard sized image, SPOT takes a centered "snapshot" of the image and saves that as a .kla file.
SPOT is meant to convert and optimize multicolor bitmaps (hi-res images get converted to multicolor).


*Compressed color RAM (.ccr) format: two adjacent half bytes are combined to reduce the size of the color RAM to 500 bytes.

**Optimized bitmap (.obm) format: bitmap info is stored column wise. Screen RAM and compressed color RAM stored row wise. First two bytes are address bytes ($00, $60) and the last one is the background color, as per koala format. File size: 9503 bytes. In most cases, this format compresses somewhat better than koala but it also needs a more complex display routine.
