# Angry Grandpa
[This is a design doc - the mod is currently in development]

In early versions of Stardew Valley (pre-1.05), grandpa's evaluation was much tougher. There were fewer ways to earn points (13 total), and 12 points were required to obtain 4 candles. If you disappointed grandpa, his original dialogue was very harsh.

![Original 1-candle dialogue](promo/original_dialogue.png)

**Angry Grandpa** mod gives the ability to restore the original dialogue or use new dialogue options! You can see your actual points total during evaluations, customize the scoring system used, reschedule grandpa's visit to happen earlier or later in the game, and even earn new rewards for achieving 1, 2, or 3 candles.

## Planned Features

### Dialogue Changes
Choose from original game dialogue, vanilla game dialogue, or creative *new* "Nuclear" dialogue for grandpa's evaluation response! Nuclear includes a lot of profanity and over-the-top enthusiasm. All dialogue variants can be made gender-neutral with an optional setting.

### Scoring System Overhaul
Choose your difficulty setting with different point thresholds. Go back to the earliest versions of the game with only 13 possible grandpa points available to earn... or use the new evaluation system with all 21 available points, but harder thresholds for earning a 4-candle result. You can also customize the schedule for grandpa's initial visit, which usually happens at the beginning of year 3.

### Display Points Total
A re-added feature from early game versions. You can now see your exact point total (out of 21 or 13 possible points). As well, you can use a diamond to request a re-evaluation and see your new score at any time, even after earning 4 candles.

### More Rewards
With bonus rewards enabled, grandpa's shrine will give you new, useful gifts for reaching milestones under 4 candles.
- 1+ candles - ancient seed artifact
- 2+ candles - dinosaur egg
- 3+ candles - prismatic shard 

If you earn 4 candles in your first evaluation, you will be given all four rewards. If you install this mod after already earning 4 candles, re-doing the evaluation will give you the bonus rewards.

### Translation Support
No translations are planned right off the bat, but the mod will be designed with full language support so that contributors can submit dialogue translations to be included in future mod updates.

## Planned Config Options
- **GrandpaDialogue:** Choose the dialogue used during evaluation and re-evaluation events. Default is `"Original"`.
    - `"Original"` - Harsher dialogue found in early versions of the game
    - `"Vanilla"` - Normal dialogue used in the game ever since version 1.05 was released
    - `"Nuclear"` - Grandpa is *very* enthusiastic about his opinions, good or bad. **Warning: profanity!**
- **GenderNeutrality:** Changes all dialogue to be gender-neutral. Defaults to `true` if [Gender Neutrality mod](https://www.nexusmods.com/stardewvalley/mods/722) is installed. Otherwise defaults to `false`.
- **ShowPointsTotal:** Shows your raw score during the evaluation (e.g. "14 of 21 Great Honors"). Defaults to `true`.
- **ScoringSystem:** Choose how points are scored and how many points are required to earn 4 candles. Default is `"Vanilla"`.
    - `"Original"` - Original game evaluation: 13 possible points, you need 12 to earn 4 candles.
    - `"Vanilla"` - Normal game evaluation: 21 possible points, you need 12 to earn 4 candles.
    - `"Hard"` - Harder scoring option. You need 18/21 points to earn 4 candles.
    - `"Expert"` - Hardest scoring option. You need all 21 points to earn 4 candles!
- **YearsBeforeEvaluation:** Default is `2`. Grandpa will make his first appearance after this many in-game years have passed.
- **BonusRewards:** Defaults to `true`. You will get new bonus rewards for earning at least 1, at least 2, and at least 3 candles. You must complete an evaluation or re-evalution event with the mod to access these rewards.
