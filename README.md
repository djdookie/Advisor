# Advisor
This is an open-source [Hearthstone Deck Tracker](https://github.com/HearthSim/Hearthstone-Deck-Tracker) plugin displaying a possible archetype deck your opponent could be playing. It also helps to track which cards remain in that deck. Popular archetype decks are downloaded from [MetaStats.net](https://www.metastats.net).

![Preview](https://github.com/kimsey0/Advisor/blob/master/AdvisorPreview.jpg "Preview")

# Description
Advisor tries to guess the opponent's deck based on the played cards.
First, the import function of Advisor allows you to quickly import hundreds of standard archetype decks from MetaStats.net. Currently the top 5 played standard decks of each archetype and each class of the last 7 days are imported.
Now while playing a Hearthstone game, with every revealed opponent card the plugin calculates similarities between those cards and all imported archetype decks.
Finally the deck with the highest match is presented to the player via overlay. If multiple decks with the same similarity are found, the most popular deck with the most played games is prioritized.
Since all played cards are additionally removed from that archetype deck, the player can see which cards the opponent supposably has left in his deck or hand with a certain likelihood.

# Installation
- Download the [latest release](https://github.com/kimsey0/Advisor/releases) (take the Advisor.zip file, not the source code!)
- Unzip it into HDT plugins folder (Options->Plugins->Plugins folder)
- Restart HDT and enable the plugin (Options->Tracker->Plugins->Advisor->Enabled)
- Import standard archetype decks (Plugins->Advisor->Import archetype decks), repeat this once a day on demand
- Play the game and enjoy
- Optional: Check the plugins settings to fit your preferences
- Hint: Move the secrets overlay position of HDT a bit to the right to not overlap with Advisors overlay window (Options->Overlay->General->Unlock overlay)

# Donation
If you like Advisor and want to support it, you can contribute here or make a [donation to the original creator, djdookie](https://paypal.me/djdookie). Thank you!

# Disclaimer
This software is provided by Dookie and other voluntary contributors "as is" and "with all faults." They make no representations or warranties of any kind concerning the safety, suitability, lack of viruses, inaccuracies, typographical errors, or other harmful components of this software. There are inherent dangers in the use of any software, and you are solely responsible for determining whether this software is compatible with your equipment and other software installed on your equipment. You are also solely responsible for the protection of your equipment and backup of your data, and no one but you will be liable for any damages or problems you may suffer in connection with using, modifying, or distributing this software.
Only use this software on your own risk and if you understand and accept this disclaimer!

# License information
This software is licensed under [GPLv3](https://www.gnu.org/licenses/gpl-3.0). Copyright 2017 © Dookie

# Special thanks
This project is open source, like all the great stuff it is based on or inspired by.
I especially want to thank the people behind Hearthstone Deck Tracker, MetaStats.net and some other HDT plugins like EndGame. Think about sharing your game results using the [MetaStats plugin](https://metastats.net/plugins) to improve our collective knowledge base!

[![GitHub Latest](https://img.shields.io/github/release/kimsey0/advisor.svg)](https://github.com/kimsey0/Advisor/releases/latest)
[![Github Latest Downloads](https://img.shields.io/github/downloads/kimsey0/advisor/latest/total.svg)](https://github.com/kimsey0/Advisor/releases/latest)
[![Github All Downloads](https://img.shields.io/github/downloads/kimsey0/advisor/total.svg)](https://github.com/kimsey0/Advisor/releases)
