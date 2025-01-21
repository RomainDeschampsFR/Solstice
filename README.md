# SOLSTICE
* Mod originaly made by WulfMarius, improved in many ways by JoeBuckley and now ressurected.
* It adds "seasons" into the game (Don't expect lush vegetation everywhere - it'll never happen...)
* Meaning the days are shorter and colder during winter, longer and warmer during summer.
* The greater the latitude, the longer the days in summer and the shorter in winter.
* **Not compatible with RELENTLESS NIGHT**.
* **Not compatible with EXTREME DROP TEMPEREATURE (Features already included)**.

![solstice](https://github.com/RomainDeschampsFR/Solstice/assets/38351288/39e3e8ef-99d8-4c9c-940e-a9344c260c53)

### SETTINGS ARE SANDBOX SPECIFIC
* Meaning saves (Old & New) aren't impacted by the mod
* To apply the mod you need to be in a particular sandbox, make the changes and confirm (see console)
* The settings will be loaded afterwards for this specific sandbox
* You can disable the mod or update its settings in the current sandbox at anytime (try not to spam it, you might end up with some weird consequences (Although those consequences are very edge cases and not game-breaking)

## FEATURES

### WARMTH BONUS IN DIRECT SUNLIGHT (OPTIONAL)
* When standing in direct sunlight you'll get a warmth bonus
* The strength of the bonus depends on :
	* the weather : Clear (6) - Partly Cloudy (3) - Cloudy (0.1) - Light Fog (0.3)
	* The sun inclinaison (highest at noon, lowest at sunrise/sunset)


### POLAR DAYS & POLAR NIGHTS
* Above 65° latitude you'll get polar days & polar nights during your run.
* Based on realistic values, here are the number of polar days/nights for each latitude (using a 365-day cycle as a reference)

Latitude	|	Polar days	|	Polar nights
-------	|	-------	|	-------
65	|	0	|	0
66	|	18	|	0
67	|	38	|	0
68	|	52	|	25
69	|	62	|	41
70	|	72	|	52
71	|	80	|	62
72	|	88	|	70
73	|	96	|	78
74	|	102	|	85
75	|	108	|	92
76	|	115	|	98
77	|	121	|	104
78	|	127	|	110
79	|	132	|	116
80	|	138	|	122

Meaning at latitude 72, you'll get every year, 88 polar days in a row and 70 polar nights in a row.

![latitude_DayLength](https://github.com/RomainDeschampsFR/Solstice/assets/38351288/73d8a4bd-ac25-41a5-b8fc-f0430b19190d)

### TIMEWIDGET UPDATED (Top right corner)
* In order to take into account polar days/nights, the time widget behavior has been changed.
* Transition between day and night is showed by the absence of sun/moon in the time widget.
* In other words, the night/day ends when the moon/sun has completely disappeared on the right.
* Therefore, the night/day starts when the moon/sun starts to show up on the left.
  
![timewidget](https://github.com/RomainDeschampsFR/Solstice/assets/38351288/009c50a1-0ee8-4825-8044-69e244da8897)

### TEMPERATURE
* The mod allows you to adjust the seasonal temperature gap as well as a flat temperature offset
* Temperature Drop Overtime already implemented in the game can be tweaked as well
* About the daily temperature gap (cannot be changed):
  * During the day, the temperature will always start to drop at 18:00 [vanilla = 15:00].
  * Depending on the day of the year and the chosen latitude, the temperature will start to rise between 00:00 (polar day in summer) and 12:00 (polar night in winter) [Vanilla = 07:00].  
  * The daily temperature difference will be progressively reduced by the length of the day up to 50% (during polar days/nights).

### AURORA
* Every things related to the day/night cycle (temperature, lights, aurora, glimmer fog, ...) is therefore impacted.
* In the specific case of auroras, their frequencies are increased during equinoxes. In short, vanilla values during Winter/Summer, twice as many during Spring/Autumn.

## Installation

* If you haven't done so already, install MelonLoader by downloading and running MelonLoader.Installer.exe.
* Download the Solstice ZIP file containing 3 DLLs : 
  * Solstice.dll
  * Innovative.Geometry.Angle.dll
  * Innovative.SolarCalculator.dll
* Move them **(The 3 of them!)** into the Mods folder in your TLD install directory.
* ModSettings required
* ModData required
