using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
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
        string[] imagePaths;
        BitmapImage[] imageArray = new BitmapImage[6];
        BitmapImage[] solutionImages = new BitmapImage[4];
        string[] solution = new string[4];
        string[] options = { "Bulbasaur", "Charmander", "Eevee", "Meowth", "Pikachu", "Squirtle" };
        string[] highscores = new string[15];
        string username;
        int attempts, maxAttempts, currentRow, score, playerIndex;
        bool debugMode, hasWon;
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
            playerIndex = 0;
            StartGame();

            // Get pictures from assets folder and put them in the BitmapImages array.
            imagePaths = Directory.GetFiles("../../assets", "*.png");

            for (int i = 0; i < imagePaths.Length; i++)
            {
                imageArray[i] = new BitmapImage(new Uri(imagePaths[i], UriKind.Relative));
            }
        }

        /// <summary>
        /// Initializes and starts a new game, resetting relevant variables and UI elements.
        /// </summary>
        private void StartGame()
        {

            username = Interaction.InputBox("Username: ", "Choose your username");
            while (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Choose a username.", "Invalid username");
                username = Interaction.InputBox("Username: ", "Choose your username");
            }

            attempts = 0;
            maxAttempts = 0;
            ChooseMaxAttempts();
            currentRow = 0;
            score = 100;
            debugMode = false;
            solutionTextBox.Visibility = Visibility.Hidden;
            hasWon = false;
            InitalizeColors();

            for (int i = 0; i < solutionImages.Length; i++)
            {
                solutionImages[i] = new BitmapImage();
            }

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
        /// Initializes the colors used for the solution by generating random colors.
        /// </summary>
        private void InitalizeColors()
        {
            GenerateRandomColor();
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
        /// Changes the background image of a label based on the ComboBox selection.
        /// </summary>
        /// <param name="ComboBox">The ComboBox whose selection determines the label background image.</param>
        /// <returns>A BitmapImage representing the selected background image.</returns>
        private BitmapImage ChangeLabelBackgroundColor(ComboBox ComboBox)
        {
            switch (ComboBox.SelectedIndex)
            {
                case 0:
                    return imageArray[0];
                case 1:
                    return imageArray[1];
                case 2:
                    return imageArray[2];
                case 3:
                    return imageArray[3];
                case 4:
                    return imageArray[4];
                case 5:
                    return imageArray[5];
                default:
                    return null;
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
        private void GenerateRandomColor()
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
                attempts++;
                CreateRow();
                UpdateLabels();
                StartCountdown();

                if (attempts == maxAttempts && !hasWon)
                {
                    highscores[playerIndex] = $"{username} - {attempts}/{maxAttempts} attempts - {score}/100";
                    checkButton.Content = "Game Over";
                    MessageBoxResult result = MessageBox.Show($"Game Over.\nThe code was:\n{String.Join(", ", solution)}", "Game over", MessageBoxButton.OK, MessageBoxImage.Question);
                    playerIndex++;
                }
                else if (hasWon)
                {
                    highscores[playerIndex] = $"{username} - {attempts}/{maxAttempts} attempts - {score}/100";
                    checkButton.Content = "Victory";
                    MessageBoxResult result = MessageBox.Show($"You won in {attempts} attempts.", "You won", MessageBoxButton.OK, MessageBoxImage.Question);
                    playerIndex++;
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
            }
            else if (solution.Contains(combobox.Text) && !ColorInCorrectPosition(combobox, position))
            {
                score -= 1;
                imageLabel.BorderBrush = Brushes.Wheat;
                imageLabel.BorderThickness = new Thickness(2);
            }
            else
            {
                imageLabel.BorderBrush = Brushes.DarkRed;
                imageLabel.BorderThickness = new Thickness(2);
            }
        }

        /// <summary>
        /// Handles the Afsluiten menuItem click event to close the program.
        /// </summary>
        /// <param name="sender">The menuItem in "Bestand" that's named "Afsluiten"</param>
        /// <param name="e">"The actual clicking on the menuItem</param>
        private void Afsluiten_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            PauseCountdown();
            sb.Clear();
            for (int i = 0; i < playerIndex; i++)
            {
                sb.Append($"{highscores[i]}\n");
            }

            MessageBox.Show($"Highscores:\n{sb.ToString()}");
            ResumeCountdown();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NieuwSpel_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AantalPogingen_Click(object sender, RoutedEventArgs e)
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
                playerGuess.Background = labels[i].Background;
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

            bool isValidAttempts = int.TryParse(Interaction.InputBox("Amount of attempts: ", "Choose between 3 and 20."), out maxAttempts);

            while (!isValidAttempts || maxAttempts < 3 || maxAttempts > 20 || maxAttempts < attempts)
            {
                isValidAttempts = int.TryParse(Interaction.InputBox("Amount of attempts: ", "Choose between 3 and 20."), out maxAttempts);
            }

            if (maxAttempts > 8 && !splitScreen)
            {
                HistoryGrid.ColumnDefinitions.Clear();
                for (int i = 0; i < 8; i++)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                    HistoryGrid.ColumnDefinitions.Add(columnDefinition);
                }
                splitScreen = true;
            }
            else if (maxAttempts <= 8 || splitScreen)
            {
                HistoryGrid.ColumnDefinitions.Clear();
                for (int i = 0; i < 4; i++)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                    HistoryGrid.ColumnDefinitions.Add(columnDefinition);
                }
                splitScreen = false;
            }

            attemptsLabel.Content = $"Attempt: {attempts} / {maxAttempts}";
            attemptsLabel.Foreground = attempts >= (maxAttempts * 0.8) ? Brushes.Red : attempts >= (maxAttempts * 0.5) ? Brushes.Orange : Brushes.Black;
            attemptsLabel.FontWeight = attempts >= (maxAttempts * 0.8) ? FontWeights.Bold : attempts >= (maxAttempts * 0.5) ? FontWeights.DemiBold : FontWeights.Normal;
            scoreLabel.Content = $"Score: {score} / 100";
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartCountdown()
        {
            remainingTime = totalTime;
            timer.Start();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingTime -= timer.Interval;
            timerTextBox.Text = $"Timer: {remainingTime.TotalSeconds.ToString("N2")} / 10";

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
        /// 
        /// </summary>
        private void StopCountdown()
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;
        }

        /// <summary>
        /// 
        /// </summary>
        private void PauseCountdown()
        {
            timer.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResumeCountdown()
        {
            timer.Start();
        }
    }
}
