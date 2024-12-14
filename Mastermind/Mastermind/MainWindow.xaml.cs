using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mastermind
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Variables
        StringBuilder sb = new StringBuilder();
        Random rnd = new Random();

        ComboBox[] comboBoxes;
        
        Label[] labels;
        
        BitmapImage[] imageArray = new BitmapImage[6];
        BitmapImage[] idleImage = new BitmapImage[1];
        BitmapImage[] solutionImages = new BitmapImage[4];

        List<string> playerNames = new List<string>();

        List<int> possibleOptions = new List<int>();

        string[] imagePaths;
        string[] idlePokemons;
        string[] solution = new string[4];
        string[] options = { "Bulbasaur", "Charmander", "Eevee", "Meowth", "Pikachu", "Squirtle" };
        string[] highscores;

        string username;

        int attempts, maxAttempts, currentRow, score, playerIndex, amountOfPlayers;

        bool debugMode, hasWon;
        bool addNewPlayer = true;
        bool splitScreen = false;

        DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);
        TimeSpan totalTime = TimeSpan.FromSeconds(10);
        TimeSpan remainingTime;

        public MainWindow()
        {
            InitializeComponent();
            comboBoxes = new ComboBox[4] { ComboBoxOption1, ComboBoxOption2, ComboBoxOption3, ComboBoxOption4 };
            comboBoxes = AddComboBoxItems(comboBoxes);
            labels = new Label[4] { colorLabel1, colorLabel2, colorLabel3, colorLabel4, };
        }

        /// <summary>
        /// Handles the Window Loaded event. Initializes game data and loads images from the assets folder.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">Event data for the Loaded event.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Defines the tick of the timer.
            timer.Tick += Timer_Tick;

            playerIndex = 0;

            StartGame();

            // Get pictures from assets folder and put them in the BitmapImages array.
            imagePaths = Directory.GetFiles("../../assets/choices", "*.png");
            idlePokemons = Directory.GetFiles("../../assets/idle", "*.png");

            for (int i = 0; i < imagePaths.Length; i++)
            {
                imageArray[i] = new BitmapImage(new Uri(imagePaths[i], UriKind.Relative));
            }

            for (int i = 0; i < idlePokemons.Length; i++)
            {
                idleImage[i] = new BitmapImage(new Uri(idlePokemons[i], UriKind.Relative));
            }
        }

        /// <summary>
        /// Initializes and starts a new game, resetting relevant variables and UI elements.
        /// </summary>
        private void StartGame()
        {
            playerNames.Clear();
            playerNameGrid.ColumnDefinitions.Clear();
            playerNameGrid.Children.Clear();

            ResetPossibleOptionsList();

            amountOfPlayers = 0;
            playerIndex = 0;
            highscores = new string[15];
            addNewPlayer = true;

            do
            {
                username = Interaction.InputBox("Username: ", "Choose your username");
                while (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Choose a username.", "Invalid username");
                    username = Interaction.InputBox("Username: ", "Choose your username");
                }

                playerNames.Add(username);
                AddPlayerToPlayerNameGrid(username);

                MessageBoxResult result =
                    MessageBox.Show("Would you like to add another player?", "Add another player", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    addNewPlayer = false;
                }

            } while (addNewPlayer);



            attempts = 0;
            maxAttempts = 0;
            ChooseMaxAttempts();
            currentRow = 0;
            score = 100;
            debugMode = false;
            solutionTextBox.Visibility = Visibility.Hidden;
            hasWon = false;
            GenerateRandomCode();

            CreateImagesToChooseFromInCode();



            UpdateLabels();
            ClearGridSection();
            ClearComboBoxSelection(labels);
            checkButton.Content = "Check code";
            if (attempts != maxAttempts)
            {
                StartCountdown();
            }
        }

        /// <summary>
        /// Adds the selectable items to the ComboBoxes.
        /// </summary>
        /// <param name="comboBoxes">An array of ComboBox elements to populate with selectable options.</param>
        /// <returns>An array of ComboBox elements with items added.</returns>
        private ComboBox[] AddComboBoxItems(ComboBox[] comboBoxes)
        {
            for (int i = 0; i < comboBoxes.Length; i++)
            {
                for (int j = 0; j < options.Length; j++)
                {
                    comboBoxes[i].Items.Add(options[j]);
                }
            }
            return comboBoxes;
        }

        /// <summary>
        /// Toggles debug mode when the Control + F12 key combination is pressed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The Key_Down event</param>
        private void ToggleDebug(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12 && !debugMode)
            {
                solutionTextBox.Visibility = Visibility.Visible;
                debugMode = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F12 && debugMode)
            {
                solutionTextBox.Visibility = Visibility.Hidden;
                debugMode = false;
            }
        }

        /// <summary>
        /// Updates the background of labels based on the corresponding selected ComboBox option.
        /// </summary>
        /// <param name="sender">The event sender (ComboBox).</param>
        /// <param name="e">The changing of the selected item of the ComboBox</param>
        private void ComboBoxOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox != null && comboBox.SelectedIndex >= 0 && comboBox.SelectedIndex < imageArray.Length)
            {
                if (comboBox == ComboBoxOption1)
                {
                    ImageBrush brush1 = new ImageBrush();
                    brush1.ImageSource = imageArray[comboBox.SelectedIndex];
                    colorLabel1.Background = brush1;
                }
                else if (comboBox == ComboBoxOption2)
                {
                    ImageBrush brush2 = new ImageBrush();
                    brush2.ImageSource = imageArray[comboBox.SelectedIndex];
                    colorLabel2.Background = brush2;
                }
                else if (comboBox == ComboBoxOption3)
                {
                    ImageBrush brush3 = new ImageBrush();
                    brush3.ImageSource = imageArray[comboBox.SelectedIndex];
                    colorLabel3.Background = brush3;
                }
                else
                {
                    ImageBrush brush4 = new ImageBrush();
                    brush4.ImageSource = imageArray[comboBox.SelectedIndex];
                    colorLabel4.Background = brush4;
                }
            }
            else
            {
                colorLabel1.Background = null;
                colorLabel2.Background = null;
                colorLabel3.Background = null;
                colorLabel4.Background = null;
            }
        }

        /// <summary>
        /// Updates the labels displaying the player's attemps and score.
        /// </summary>
        private void UpdateLabels()
        {
            attemptsLabel.Content = $"Attempt: {attempts} / {maxAttempts}";
            attemptsLabel.Foreground = attempts >= (maxAttempts * 0.8) ? Brushes.Red : attempts >= (maxAttempts * 0.5) ? Brushes.Orange : Brushes.Black;
            attemptsLabel.FontWeight = attempts >= (maxAttempts * 0.8) ? FontWeights.Bold : attempts >= (maxAttempts * 0.5) ? FontWeights.DemiBold : FontWeights.Normal;
            scoreLabel.Content = $"Score: {score} / 100";
            solutionTextBox.Text = String.Join(", ", solution);
            ShowActivePlayer();
        }

        /// <summary>
        /// Clears the selection of all ComboBoxes and resets the corresponding label borders.
        /// </summary>
        /// <param name="labels">An array of Label elements to reset.</param>
        private void ClearComboBoxSelection(Label[] labels)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                comboBoxes[i].SelectedValue = 2;
                labels[i].BorderBrush = null;
            }
        }

        /// <summary>
        /// Generates a random solution for the game by selecting random images within the amount of options.
        /// </summary>
        private void GenerateRandomCode()
        {
            for (int i = 0; i < 4; i++)
            {
                int random = rnd.Next(0, imageArray.Length);
                solution[i] = options[random];
                solutionImages[i] = imageArray[random];
            }
        }

        /// <summary>
        /// Handles the Check button click event to validate the player's guess and update the game state.
        /// </summary>
        /// <param name="sender">The button on which the user clicks.</param>
        /// <param name="e">The actual click of the button.</param>
        private void checkButton_Click(object sender, RoutedEventArgs e)
        {
            if (attempts != maxAttempts && !hasWon)
            {
                CheckIfPlayerHasWon();
                IfCorrectPositionRemoveOptionFromHints();
                attempts++;
                CreateRow();
                UpdateLabels();
                StartCountdown();

                if (attempts == maxAttempts && !hasWon && playerIndex + 1 < playerNames.Count)
                {
                    highscores[playerIndex] =
                        $"{playerNames[playerIndex]}" +
                        $" - {attempts}/{maxAttempts} attempts" +
                        $" - {score}/100";
                    checkButton.Content = "Game Over";
                    MessageBoxResult result = MessageBox.Show($"Game Over.\n" +
                        $"The code was: {String.Join(", ", solution)}\n" +
                        $"Next up is {playerNames[playerIndex + 1]}",
                        $"{playerNames[playerIndex]}",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
                    playerIndex++;
                    ResetGame();

                }
                else if (attempts == maxAttempts && !hasWon && playerIndex + 1 >= playerNames.Count)
                {
                    highscores[playerIndex] =
                        $"{playerNames[playerIndex]}" +
                        $" - {attempts}/{maxAttempts} attempts" +
                        $" - {score}/100";
                    checkButton.Content = "Game Over";
                    MessageBoxResult result = MessageBox.Show($"Game Over.\n" +
                        $"The code was: {String.Join(", ", solution)}\n" +
                        $"{playerNames[playerIndex]} was the last player.",
                        $"{playerNames[playerIndex]}",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
                }
                else if (hasWon && playerIndex + 1 < playerNames.Count)
                {
                    highscores[playerIndex] =
                        $"{playerNames[playerIndex]}" +
                        $" - {attempts}/{maxAttempts} attempts" +
                        $" - {score}/100";
                    checkButton.Content = "Victory";
                    MessageBoxResult result = MessageBox.Show($"You won in {attempts} attempts.\n" +
                        $"The code was: {String.Join(", ", solution)}\n" +
                        $"Next up is {playerNames[playerIndex + 1]}",
                        "You won",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
                    playerIndex++;
                    ResetGame();
                }
                else if (hasWon && playerIndex + 1 >= playerNames.Count)
                {
                    highscores[playerIndex] =
                        $"{playerNames[playerIndex]}" +
                        $" - {attempts}/{maxAttempts} attempts" +
                        $" - {score}/100";
                    checkButton.Content = "Victory";
                    MessageBoxResult result = MessageBox.Show($"You won in {attempts} attempts.\n" +
                        $"The code was: {String.Join(", ", solution)}\n" +
                        $"{playerNames[playerIndex]} was the last player.",
                        "You won",
                        MessageBoxButton.OK,
                        MessageBoxImage.Question);
                }
            }
        }

        /// <summary>
        /// Checks the player's code by comparing the selected text in the ComboBox with the solution.
        /// Updates the image label's border and subtracts score based on the correctness of the guess.
        /// </summary>
        /// <param name="combobox">The ComboBox containing the player's selected color.</param>
        /// <param name="imageLabel">The Label representing the image slot in the game UI.</param>
        /// <param name="position">The position of the ComboBox in the solution sequence.</param>
        private void CheckCode(ComboBox combobox, Label imageLabel, int position)
        {
            if (combobox.Text == null || !solution.Contains(combobox.Text))
            {
                score -= 2;
                imageLabel.BorderThickness = new Thickness(0);
                imageLabel.ToolTip = "Wrong Pokémon.";
            }
            else if (solution.Contains(combobox.Text) && !ColorInCorrectPosition(combobox, position))
            {
                score -= 1;
                imageLabel.BorderBrush = Brushes.Wheat;
                imageLabel.BorderThickness = new Thickness(2);
                imageLabel.ToolTip = "Right Pokémon, wrong position";
            }
            else
            {
                imageLabel.BorderBrush = Brushes.DarkRed;
                imageLabel.BorderThickness = new Thickness(2);
                imageLabel.ToolTip = "Right Pokémon, right position";
            }
        }

        /// <summary>
        /// Handles the Close menuItem click event to close the program.
        /// </summary>
        /// <param name="sender">The menuItem in "Bestand" that's named "Afsluiten"</param>
        /// <param name="e">"The actual clicking on the menuItem</param>
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Highscores menuItem click event to show the highscores.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            PauseCountdown();
            sb.Clear();
            for (int i = 0; i <= playerIndex; i++)
            {
                sb.Append($"{highscores[i]}\n");
            }

            MessageBox.Show($"Highscores:\n{sb.ToString()}");
            ResumeCountdown();
        }

        /// <summary>
        /// Starts a new game when the player clicks on "New game" in the File dropdown menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        /// <summary>
        /// Prompts the user for a number as the amount of attempts to guess the correct code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AmountAttempts_Click(object sender, RoutedEventArgs e)
        {
            PauseCountdown();
            ChooseMaxAttempts();
            ResumeCountdown();
        }

        /// <summary>
        /// Checks if the text selected in the ComboBox is in the correct position within the solution.
        /// </summary>
        /// <param name="combobox">The ComboBox containing the player's selected color in text.</param>
        /// <param name="position">The expected position of the color in the solution sequence.</param>
        /// <returns>True if the color is in the correct position; otherwise, false.</returns>
        private bool ColorInCorrectPosition(ComboBox combobox, int position)
        {
            return combobox.Text == solution[position];
        }

        /// <summary>
        /// Creates a new row in the game history grid to display the player's guesses.
        /// Calls CheckCode() to validate each guess and updates the grid accordingly.
        /// </summary>
        private void CreateRow()
        {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = new GridLength(1, GridUnitType.Star);
            HistoryGrid.RowDefinitions.Add(rowDefinition);

            for (int i = 0; i < 4; i++)
            {
                ComboBox combobox = comboBoxes[i];
                Label playerGuess = new Label();
                if (combobox.SelectedIndex != -1 || combobox.SelectedItem != null)
                {
                    playerGuess.Background = labels[i].Background;
                } 
                else
                {
                    ImageBrush idleGuess = new ImageBrush();
                    idleGuess.ImageSource = idleImage[0];
                    playerGuess.Background = idleGuess;
                }
                
                playerGuess.Margin = new Thickness(1);

                // Change the size of images depending on how many guesses a player can make.
                playerGuess.Height = 50;
                playerGuess.Width = 50;

                // Use the extra columnDefintion if it has been created, because of more than 16 possible attempts.
                if (maxAttempts < 16)
                {
                    if (currentRow < 8)
                    {
                        Grid.SetRow(playerGuess, currentRow);
                        Grid.SetColumn(playerGuess, i);
                    }
                    else
                    {
                        Grid.SetRow(playerGuess, currentRow - 8);
                        Grid.SetColumn(playerGuess, i + 4);
                    }
                }
                else
                {
                    if (currentRow < 10)
                    {
                        Grid.SetRow(playerGuess, currentRow);
                        Grid.SetColumn(playerGuess, i);
                    }
                    else
                    {
                        Grid.SetRow(playerGuess, currentRow - 10);
                        Grid.SetColumn(playerGuess, i + 4);
                    }
                }


                HistoryGrid.Children.Add(playerGuess);

                CheckCode(combobox, playerGuess, i);


            }
            currentRow++;
        }

        /// <summary>
        /// Clears the game history grid by removing all row definitions and child elements.
        /// This method resets the grid section, preparing it for a new game.
        /// </summary>
        private void ClearGridSection()
        {
            HistoryGrid.RowDefinitions.Clear();
            HistoryGrid.Children.Clear();
        }

        /// <summary>
        /// Checks if the text in the ComboBox corresponds with the text in the solution string at its respective index.
        /// </summary>
        /// <returns>If the text is the same in all places, sets hasWon to true; otherwise, returns false.</returns>
        private bool CheckIfPlayerHasWon()
        {
            if (ComboBoxOption1.Text == solution[0] &&
                ComboBoxOption2.Text == solution[1] &&
                ComboBoxOption3.Text == solution[2] &&
                ComboBoxOption4.Text == solution[3])
            {
                return hasWon = true;
            }
            else
            {
                return hasWon = false;
            }
        }

        /// <summary>
        /// Allows the player(s) to choose their number of attempts at the start of the game.
        /// The choice that's made here is used for all players for the length of the overall game.
        /// </summary>
        private void ChooseMaxAttempts()
        {

            maxAttempts = GetValidAmountOfAttempts();
            UpdateGridLayout(maxAttempts);
            UpdateLabels();
        }

        /// <summary>
        /// Set the remaining time to the total available time of each round. Start the timer with an interval of 100 milliseconds. Sets the Timer_Tick function as what happens every 100 milliseconds.
        /// </summary>
        private void StartCountdown()
        {
            remainingTime = totalTime;
            timer.Start();
            timer.Interval = TimeSpan.FromMilliseconds(100);
        }

        /// <summary>
        /// Subtracts the timer interval from the remaining time. When remaining time reaches zero, the checkButton_Click() is called for an attempt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime -= timer.Interval;
            timerTextBox.Text = $"Timer: {(int)remainingTime.TotalSeconds} / 10";

            if (attempts < maxAttempts)
            {
                if (hasWon)
                {
                    StopCountdown();
                }
                else if (remainingTime <= TimeSpan.Zero)
                {
                    StopCountdown();
                    checkButton_Click(null, null);
                }
            }
            else if (attempts == maxAttempts)
            {
                StopCountdown();
            }
            UpdateLabels();
        }

        /// <summary>
        /// Stops the countdown and removes the tick from the timer.
        /// </summary>
        private void StopCountdown()
        {
            timer.Stop();
        }

        /// <summary>
        /// Pauses the countdown timer.
        /// </summary>
        private void PauseCountdown()
        {
            timer.Stop();
        }

        /// <summary>
        /// Resumes the countdown timer.
        /// </summary>
        private void ResumeCountdown()
        {
            timer.Start();
        }

        private void buyHintButton_Click(object sender, RoutedEventArgs e)
        {
            PauseCountdown();

            MessageBoxResult hintChoice = MessageBox.Show(
                "Choose your hint:\n" +
                "\"Yes\" - Pokémon in correct position (-25 points)\n" +
                "\"No\" - Pokémon present but position unknown (-15 points)\n" +
                "\"Cancel\" - No hint.",
                "Hint Options",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (hintChoice == MessageBoxResult.Yes)
            {
                // Hint for correct image in the correct position
                int position = rnd.Next(0, possibleOptions.Count);
                if (possibleOptions.Count > 0 && score >= 25)
                {
                    int correctPosition = possibleOptions[position];
                    MessageBox.Show($"Correct Pokémon in position {correctPosition + 1}: {solution[correctPosition]}");
                    score -= 25;

                    // Remove this position from future hints
                    possibleOptions.Remove(correctPosition);
                }
                else if (score < 25)
                {
                    MessageBox.Show("Not enough points to spend.");
                }
                else
                {
                    MessageBox.Show("No more hints to show.");
                }

            }
            else if (hintChoice == MessageBoxResult.No && score >= 15)
            {
                if (score >= 15)
                {
                    // Hint for a correct image, but not the position
                    string hintImage = solution[rnd.Next(0, solution.Length)];
                    MessageBox.Show($"A Pokémon present in the solution: {hintImage}");
                    score -= 15;
                }
                else if (score < 15)
                {
                    MessageBox.Show("Not enough points to spend.");
                }
            }
            else
            {
                // No hint selected
                MessageBox.Show("No hint selected.");
            }

            StartCountdown();
            UpdateLabels();
        }

        private void ResetGame()
        {
            attempts = 0;
            currentRow = 0;
            score = 100;
            debugMode = false;
            solutionTextBox.Visibility = Visibility.Hidden;
            hasWon = false;
            GenerateRandomCode();
            ResetPossibleOptionsList();

            UpdateLabels();
            ClearGridSection();
            ClearComboBoxSelection(labels);
            checkButton.Content = "Check code";
            if (attempts != maxAttempts)
            {
                StartCountdown();
            }
        }

        private void CreateImagesToChooseFromInCode()
        {
            for (int i = 0; i < solutionImages.Length; i++)
            {
                solutionImages[i] = new BitmapImage();
            }
        }

        private void AddPlayerToPlayerNameGrid(string username)
        {
            ColumnDefinition newPlayerColumn = new ColumnDefinition();
            newPlayerColumn.Width = new GridLength(1, GridUnitType.Star);

            Label playerNameLabel = new Label();
            playerNameLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
            playerNameLabel.VerticalContentAlignment = VerticalAlignment.Center;
            playerNameLabel.FontSize = 10;

            playerNameGrid.ColumnDefinitions.Add(newPlayerColumn);
            Grid.SetColumn(playerNameLabel, playerNames.Count - 1);

            playerNameLabel.Content = username;

            playerNameGrid.Children.Add(playerNameLabel);
        }

        private void ShowActivePlayer()
        {
            foreach (Label child in playerNameGrid.Children)
            {
                // Get the column of the current child
                int column = Grid.GetColumn(child);

                // Check if the column matches the player index
                if (column == playerIndex)
                {
                    // Set the label's foreground color to green, background to white and weight to bold
                    child.Foreground = Brushes.White;
                    child.Background = Brushes.Green;
                    child.FontWeight = FontWeights.Bold;
                }
                else
                {
                    // Set the label's foreground color to default, black, background transparent and weight back to normal.
                    child.Foreground = Brushes.Black;
                    child.Background = Brushes.Transparent;
                    child.FontWeight = FontWeights.Normal;
                }
            }
        }

        private int GetValidAmountOfAttempts()
        {
            int attemptsInput;
            bool isValid;
            do
            {
                string input = Interaction.InputBox("Amount of attempts: ", "Choose between 3 and 20.");
                isValid = int.TryParse(input, out attemptsInput) && attemptsInput >= 3 && attemptsInput <= 20 && attemptsInput >= attempts;
            } while (!isValid);

            return attemptsInput;
        }

        private void UpdateGridLayout(int maxAttempts)
        {
            int columnCount;
            // Decide the number of columns based on maxAttempts
            if (maxAttempts > 8)
                columnCount = 8;
            else
                columnCount = 4;

            // Clear and recreate the grid columns
            HistoryGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < columnCount; i++)
            {
                ColumnDefinition columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                HistoryGrid.ColumnDefinitions.Add(columnDefinition);
            }

            // Set splitScreen true or false based on the maxAttempts;
            if (maxAttempts > 8)
                splitScreen = true;
            else
                splitScreen = false;
        }

        private void ResetPossibleOptionsList()
        {
            possibleOptions = new List<int> { 0, 1, 2, 3 };
        }

        private void IfCorrectPositionRemoveOptionFromHints()
        {
            if (ComboBoxOption1.Text == solution[0])
            {
                possibleOptions.Remove(0);
            }

            if (ComboBoxOption2.Text == solution[1])
            {
                possibleOptions.Remove(1);
            }

            if (ComboBoxOption3.Text == solution[2]) 
            {
                possibleOptions.Remove(2);
            }

            if (ComboBoxOption4.Text == solution[3])
            {
                possibleOptions.Remove(3);
            }

            foreach (int i in possibleOptions)
            {
                Console.WriteLine(i);
            }
        }
    }
}
