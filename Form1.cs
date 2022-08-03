using System.ComponentModel;
using System.Text;

namespace DivotDerbyLeaderboard
{
    public partial class FormDivotDerbyLeaderboard : Form
    {
        List<Champion> champions = new List<Champion>();
        List<Label> championLabels = new List<Label>();
        List<Label> winLabels = new List<Label>();
        List<Champion> formerChampions = new List<Champion>();
        bool isBoardShowingToday = true;
        int overallPageCount = 0;
        private bool isWinnerCrowned = false;
        string version = "1.10";

        public FormDivotDerbyLeaderboard()
        {
            InitializeComponent();
            updateTitleToIncludeVersion(version);
            GetChampionLabels();
            GetWinLabels();
            LoadSaved();
            UpdateLeaderboard(champions);
            BackupData();
        }

        private async void LabelClickEvent(object sender, EventArgs e)
        {
            if (isBoardShowingToday)
            {
                Label thisLabel = (Label)sender;
                string playerName = thisLabel.Text;
                MouseEventArgs thisEvent = (MouseEventArgs)e;
                if (thisEvent.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    ReduceTotal(playerName);
                }
                else if (thisEvent.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    IncrementTotal(playerName);
                }
                await UpdateLeaderboard(champions);
            }
        }

        private void ChangeTotal(string playerName, int amount)
        {
            Champion? champion = champions.FirstOrDefault(champ => champ.Name == playerName);
            if (champion != null)
            {
                champion.Wins += amount;
                if (champion.Wins <= 0)
                {
                    champions.Remove(champion);
                }
            }
        }

        private void IncrementTotal(string playerName)
        {
            ChangeTotal(playerName, 1);
        }

        private void ReduceTotal(string playerName)
        {
            ChangeTotal(playerName, -1);
        }

        private async void AddNewChampion()
        {
            if (this.TextBoxNewChampion.Text.Length > 0)
            {
                Champion newChampion = new Champion(this.TextBoxNewChampion.Text);
                champions.Add(newChampion);
                this.TextBoxNewChampion.Text = String.Empty;
            }
            await UpdateLeaderboard(champions);
        }

        private void ButtonAddNewChampion_Click(object sender, EventArgs e)
        {
            AddNewChampion();
        }

        private async Task UpdateLeaderboard(List<Champion> myChampions)
        {
            myChampions = myChampions.Where(champion => champion.Wins > 0).ToList();
            if (myChampions.Count > 0)
            {
                if (championLabels[0].Text.Length > 0)
                {
                    await ClearLeaderboardLabels(10);
                }
                myChampions = myChampions.OrderByDescending(champ => champ.Wins).ToList();
                for (int i = 0; i < 7; i++)
                {
                    if (i < myChampions.Count)
                    {
                        championLabels[i].Text = myChampions[i].Name;
                        await Task.Delay(50);
                        winLabels[i].Text = myChampions[i].Wins.ToString();
                    }
                    else
                    {
                        championLabels[i].Text = String.Empty;
                        winLabels[i].Text = String.Empty;
                    }
                    await Task.Delay(50);
                }
            }
            else
            {
                foreach (Label l in championLabels) { l.Text = String.Empty; };
                foreach (Label l in winLabels) { l.Text = String.Empty; };
            }
            SaveToday();
        }

        private async Task ClearLeaderboardLabels(int waitTime = 50)
        {
            foreach (Label l in championLabels) { l.Text = String.Empty; await Task.Delay(waitTime); };
            foreach (Label l in winLabels) { l.Text = String.Empty; await Task.Delay(waitTime); };
        }

        private void LoadSaved()
        {
            if (File.Exists("DivotDerbyLeaderboardToday.txt"))
            {
                FileInfo todayFileInfo = new FileInfo("DivotDerbyLeaderboardToday.txt");
                    champions = new List<Champion>();
                    try
                    {
                        string text = File.ReadAllText("DivotDerbyLeaderboardToday.txt");
                        string[] entries = text.Split("|");
                        for (int i = 1; i < entries.Length; i++)
                        {
                            string[] championData = entries[i].Split(":");
                            Champion champ = new Champion(championData[0], int.Parse(championData[1]));
                            champions.Add(champ);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format("Error loading leaderboard.\r{0}", ex.Message));
                    }
            }
        }

        private void SaveToday()
        {
            StringBuilder str = new StringBuilder();

            foreach (Champion champion in champions)
            {
                str.Append(String.Format("|{0}:{1}", champion.Name, champion.Wins));
            }

            try
            {
                using (StreamWriter writer = new StreamWriter("DivotDerbyLeaderboardToday.txt", false))
                {
                    writer.WriteAsync(str.ToString());
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error saving leaderboard.\r{0}", ex.Message));
            }
        }

        private void SaveWinner()
        {
            try
            {
                GetFormerChampions();
                // add #1 champion to formerChampions
                List<Champion> newChampList = champions.OrderByDescending(c => c.Wins).ToList();
                Champion? duplicate = formerChampions.FirstOrDefault(c => newChampList[0].Name == c.Name);
                if (duplicate != null)
                {
                    duplicate.Wins++;
                }
                else
                {
                    newChampList[0].Wins = 1;
                    formerChampions.Add(newChampList[0]);
                }
                // write new overall list
                StringBuilder str = new StringBuilder();

                foreach (Champion champion in formerChampions)
                {
                    str.Append(String.Format("|{0}:{1}", champion.Name, champion.Wins));
                }
                try
                {
                    using (StreamWriter writer = new StreamWriter("DivotDerbyLeaderboardTotal.txt", false))
                    {
                        writer.WriteAsync(str.ToString());
                        writer.Close();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(String.Format("Error saving total leaderboard.\r{0}", ex.Message));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(String.Format("Unable to save total leaderboard\r{0}", ex.Message));
            }
        }

        private async void ShowNextSevenFormerChampions()
        {
            List<Champion> topFormerChampions = new List<Champion>();
            for (int i = 0; i < 7; i++)
            {
                var currentCount = (i + (7 * overallPageCount));
                if (formerChampions.Count > currentCount)
                {
                    topFormerChampions.Add(formerChampions[currentCount]);
                    SetLabelText(currentCount);
                }
                else
                {
                    SetLabelText(currentCount);
                }
            }
            overallPageCount += 1;
            if (formerChampions.Count < overallPageCount * 7)
            {
                overallPageCount = 0;
            }
            await UpdateLeaderboard(topFormerChampions);
        }

        private void GetFormerChampions()
        {
            formerChampions = new List<Champion>();
            if (File.Exists("DivotDerbyLeaderboardTotal.txt"))
            {
                string text = File.ReadAllText("DivotDerbyLeaderboardTotal.txt");
                string[] entries = text.Split("|");
                for (int i = 1; i < entries.Length; i++)
                {
                    string[] championData = entries[i].Split(":");
                    Champion champ = new Champion(championData[0], int.Parse(championData[1]));
                    formerChampions.Add(champ);
                }
            }
        }

        private void ChangeLabels()
        {
            List<Champion> firstSeven = new List<Champion>();
            formerChampions = formerChampions.OrderByDescending(c => c.Wins).ToList();
            overallPageCount = 0;
            ShowNextSevenFormerChampions();
        }

        private async void ToggleLeaderboard(string board)
        {
            SwitchButtonColors(ButtonToday);
            SwitchButtonColors(ButtonOverall);

            switch (board)
            {
                case ("Former"):
                    SaveToday();
                    GetFormerChampions();
                    if (formerChampions.Count > 0)
                    {
                        ChangeLabels();
                    }
                    else
                    {
                        await ClearLeaderboardLabels();
                    }
                    break;
                case ("Today"):
                default:
                    isWinnerCrowned = false;
                    for (int i = 0; i < 7; i++)
                    {
                        SetLabelText(i);
                    }
                    LoadSaved();
                    await UpdateLeaderboard(champions);
                    break;
            }
        }

        private void SwitchButtonColors(Button btn)
        {
            if (btn.ForeColor == Color.MediumBlue)
            {
                btn.ForeColor = Color.Lime;
                btn.BackColor = Color.Navy;
            }
            else
            {
                btn.ForeColor = Color.MediumBlue;
                btn.BackColor = Color.Black;
            }
        }

        private void GetChampionLabels()
        {
            championLabels.Add(LabelPlayer1);
            championLabels.Add(LabelPlayer2);
            championLabels.Add(LabelPlayer3);
            championLabels.Add(LabelPlayer4);
            championLabels.Add(LabelPlayer5);
            championLabels.Add(LabelPlayer6);
            championLabels.Add(LabelPlayer7);
        }

        private void GetWinLabels()
        {
            winLabels.Add(LabelWins1);
            winLabels.Add(LabelWins2);
            winLabels.Add(LabelWins3);
            winLabels.Add(LabelWins4);
            winLabels.Add(LabelWins5);
            winLabels.Add(LabelWins6);
            winLabels.Add(LabelWins7);
        }

        private void LabelPlayer1_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer2_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer3_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer4_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer5_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer6_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void LabelPlayer7_Click(object sender, EventArgs e)
        {
            LabelClickEvent(sender, e);
        }

        private void ButtonToday_Click(object sender, EventArgs e)
        {
            if (!isBoardShowingToday)
            {
                isBoardShowingToday = true;
                ToggleLeaderboard("Today");
            }
        }

        private void ButtonOverall_Click(object sender, EventArgs e)
        {
            ResetWinnerText();
            ImageBigGoldenCrown.Visible = false;
            if (isBoardShowingToday)
            {
                isBoardShowingToday = false;
                ToggleLeaderboard("Former");
            }
            else
            {
                ShowNextSevenFormerChampions();
            }
        }

        private void TextBoxNewChampion_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AddNewChampion();
            }
            e.Handled = true;
        }

        private void ImageSmallBlackCrown_Click(object sender, EventArgs e)
        {
            CrownClicked();
        }

        private void ClearLeaderboard()
        {
            try
            {
                File.Delete("DivotDerbyLeaderboardToday.txt");
                champions.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Failed to reset board\r{0}", ex.Message));
            }
        }

        private void CrownClicked()
        {
            if (!isWinnerCrowned)
            {
                SaveWinner();
                ImageBigGoldenCrown.Visible = true;
                HighlightWinnerText();
                isWinnerCrowned = true;
            }
            else
            {
                ClearLeaderboard();
                ImageBigGoldenCrown.Visible = false;
                ResetWinnerText();
                isWinnerCrowned = false;
            }
        }

        private void HighlightWinnerText()
        {
            LabelFirst.Font = new Font("Segoe UI", 35);
            LabelFirst.Location = new Point(17, 05);
            LabelPlayer1.Font = new Font("Segoe UI", 35);
            LabelPlayer1.ForeColor = Color.Gold;
            LabelPlayer1.Location = new Point(100, 05);
            LabelWins1.Font = new Font("Segoe UI", 35);
            LabelWins1.ForeColor = Color.Gold;
            LabelWins1.Location = new Point(385, 05);
        }

        private void ResetWinnerText()
        {
            LabelFirst.Font = new Font("Segoe UI", 25);
            LabelFirst.Location = new Point(17, 20);
            LabelPlayer1.Font = new Font("Segoe UI", 25);
            LabelPlayer1.ForeColor = Color.Gainsboro;
            LabelPlayer1.Location = new Point(100, 20);
            LabelWins1.Font = new Font("Segoe UI", 25);
            LabelWins1.ForeColor = Color.Gainsboro;
            LabelWins1.Location = new Point(385, 20);
        }

        private void ImageBigGoldenCrown_Click(object sender, EventArgs e)
        {
            CrownClicked();
        }

        private void SetLabelText(int currentCount)
        {
            Label thisLabel;
            switch (currentCount % 7)
            {
                default:
                case 0:
                    thisLabel = LabelFirst;
                    break;
                case 1:
                    thisLabel = LabelSecond;
                    break;
                case 2:
                    thisLabel = LabelThird;
                    break;
                case 3:
                    thisLabel = LabelFourth;
                    break;
                case 4:
                    thisLabel = LabelFifth;
                    break;
                case 5:
                    thisLabel = LabelSixth;
                    break;
                case 6:
                    thisLabel = LabelSeventh;
                    break;
            }
            string str = GetEnglishForTh(currentCount);
            thisLabel.Text = str;
        }

        private string GetEnglishForTh(int currentCount)
        {
            switch (currentCount + 1)
            {
                case 1: return "1st";
                case 2: return "2nd";
                case 3: return "3rd";
                case 21: return "21st";
                case 22: return "22nd";
                case 23: return "23rd";
                case 31: return "31st";
                case 32: return "32nd";
                case 33: return "33rd";
                case 41: return "41st";
                case 42: return "42nd";
                case 43: return "43rd";
                default:
                    return String.Format("{0}th", currentCount+1);
            }
        }

        private void BackupData()
        {
            if (File.Exists("DivotDerbyLeaderboardTotal.txt"))
            {
                File.Copy("DivotDerbyLeaderboardTotal.txt", "DivotDerbyLeaderboardTotalBackup.txt", true);
            }
            if (File.Exists("DivotDerbyLeaderboardToday.txt"))
            {
                File.Copy("DivotDerbyLeaderboardToday.txt", "DivotDerbyLeaderboardTodayBackup.txt", true);
            }
        }

        private void updateTitleToIncludeVersion(string version)
        {
            this.Text = String.Format("{0}  v{1}", this.Text, version);
        }
    }

    class Champion
    {
        public string? Name { get; set; }
        public int Wins { get; set; }

        public Champion() {
            this.Wins = 1;
        }

        public Champion(string championName)
        {
            Name = championName;
            Wins = 1;
        }

        public Champion(string championName, int championWins)
        {
            Name = championName;
            Wins = championWins;
        }
    }
}