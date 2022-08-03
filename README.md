# Divot-Derby-Leaderboard
Divot Derby Leaderboard built for ProgressiveGolf (Twitch)
https://www.twitch.tv/progressivegolf

The leaderboard should:<br>
a)  add a name to the list with 1 win upon typing and pressing enter or clicking the add button.<br>
b)  right click on a name increments their win count and resorts the list by wins.<br>
c)  left click on a name reduces their win count and resorts the list by wins.<br>
d)  if win count is reduced to 0 their name is removed from the list.<br>
e)  clicking the shadow-crown to the left of "Leaderboard" crowns a winner - this has a UI effect and adds their name to the overall leaderboard (DivotDerbyLeaderboardTotal.txt).<br>
f)  crowning the same person a second time should increment their count on the Total leaderboard.<br>
g)  if there are more than 7 people in the Total leaderboard, clicking on the "Previous" button again will scroll to the next 7.<br>
h)  names will be saved between sessions... UNTIL a winner is crowned.<br>
i)  after a winner is crowned, that lone name is saved and the current list gets cleared to begin a new session.

Variables:<br>
List<Champion> 'champion' is a list of champions on the current leaderboard with their current win count<br>
List<Champion> 'formerChampions' is a list of champions that have been crowned<br>
isBoardShowingToday is simply a boolean that confirms the current state of the leaderboard - whether it's showing the champion list or the formerChampion list<br>
