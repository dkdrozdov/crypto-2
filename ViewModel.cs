using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace C_2
{
    public class ViewModel : INotifyPropertyChanged
    {
        public Model Model { get; init; }
        public ObservableCollection<Tuple<string, decimal, string>> Words { get; set; } = new();
        private string? probabilitiesFilePath;
        private string? alphabetFilePath;
        private string? textFilePath;
        private string? resultFilePath;
        public string? ProbabilitiesFilePath
        {
            get => probabilitiesFilePath;
            set
            {
                probabilitiesFilePath = value;
                OnPropertyChanged();
                OnPathChanged();
            }
        }
        public string? AlphabetFilePath
        {
            get => alphabetFilePath;
            set
            {
                alphabetFilePath = value;
                OnPropertyChanged();
                OnPathChanged();
            }
        }
        public string? TextFilePath
        {
            get => textFilePath;
            set
            {
                textFilePath = value;
                OnPropertyChanged();
                OnPathChanged();
            }
        }
        public string? ResultFilePath
        {
            get => resultFilePath;
            set
            {
                resultFilePath = value;
                OnPropertyChanged();
                OnPathChanged();
            }
        }
        private string? isKraft;
        public string? IsKraft
        {
            get => isKraft;
            set
            {
                isKraft = value;
                OnPropertyChanged();
            }
        }
        private string? averageCodeLength;
        public string? AverageCodeLength
        {
            get => averageCodeLength;
            set
            {
                averageCodeLength = value;
                OnPropertyChanged();
            }
        }
        private string? redundancy;
        public string? Redundancy
        {
            get => redundancy;
            set
            {
                redundancy = value;
                OnPropertyChanged();
            }
        }
        public ViewModel()
        {
            Model = new();
            PathChanged += UpdateData;
        }

        private void UpdateData(object? sender, PropertyChangedEventArgs e)
        {
            if (AlphabetFilePath != null && ProbabilitiesFilePath != null &&
                File.Exists(AlphabetFilePath) && File.Exists(ProbabilitiesFilePath))
                try
                {
                    Words.Clear();
                    var words = Model.GetWords(AlphabetFilePath, ProbabilitiesFilePath);
                    foreach (var word in words)
                        Words.Add(word);
                    var codewords = Words.Select((x) => x.Item3);
                    var _averageCodeLength = Model.AverageCodeLength(codewords);
                    AverageCodeLength = $"{_averageCodeLength:F4}";
                    var _redundancy = Model.CalculateRedundancy(Words.Select((x) => x.Item2), _averageCodeLength);
                    Redundancy = $"{_redundancy:F4}";
                    IsKraft = Model.IsKraft(codewords) ? "выполняется" : "не выполняется";
                }
                catch (Exception exception) when (exception is ArgumentOutOfRangeException)
                {
                    MessageBox.Show(exception.Message);
                }
        }
        private RelayCommand? encodeCommand;

        public RelayCommand EncodeCommand
        {
            get
            {
                return encodeCommand ??
                    (encodeCommand = new RelayCommand(obj =>
                    {
                        try
                        {

                            var s = Model.Encoder.Encode(File.ReadAllText(TextFilePath!),
                                Words.ToList().Select((x) => Tuple.Create(x.Item1, x.Item3)));
                            File.WriteAllText(ResultFilePath!, s);
                            MessageBox.Show("Операция прошла успешно.");
                        }
                        catch (Exception e) when (e is ArgumentException)
                        {
                            MessageBox.Show(e.Message);
                        }
                    },
                    (obj) =>
                    AlphabetFilePath != null &&
                    ProbabilitiesFilePath != null &&
                    TextFilePath != null &&
                    ResultFilePath != null &&
                    File.Exists(AlphabetFilePath) &&
                    File.Exists(ProbabilitiesFilePath) &&
                    File.Exists(TextFilePath)));
            }
        }
        private RelayCommand? decodeCommand;

        public RelayCommand DecodeCommand
        {
            get
            {
                return decodeCommand ??
                    (decodeCommand = new RelayCommand(obj =>
                    {
                        try
                        {
                            var s = Model.Decoder.Decode(File.ReadAllText(TextFilePath!),
                                Words.ToList().Select((x) => Tuple.Create(x.Item1, x.Item3)));
                            File.WriteAllText(ResultFilePath!, s);
                            MessageBox.Show("Операция прошла успешно.");
                        }
                        catch (Exception e) when (e is ArgumentException)
                        {
                            MessageBox.Show(e.Message);
                        }
                    },
                    (obj) =>
                    AlphabetFilePath != null &&
                    ProbabilitiesFilePath != null &&
                    TextFilePath != null &&
                    ResultFilePath != null &&
                    File.Exists(AlphabetFilePath) &&
                    File.Exists(ProbabilitiesFilePath) &&
                    File.Exists(TextFilePath)));
            }
        }
        private RelayCommand? exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                return exitCommand ??
                    (exitCommand = new RelayCommand(obj =>
                    {
                        ((Window)obj).Close();
                    }));
            }
        }
        private RelayCommand? pickProbabilityPathCommand;
        public RelayCommand PickProbabilityPathCommand
        {
            get
            {
                return pickProbabilityPathCommand ??
                    (pickProbabilityPathCommand = new RelayCommand(obj =>
                    {
                        OpenFileDialog openFileDialog = new();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            ProbabilitiesFilePath = openFileDialog.FileName;
                        }
                    }));
            }
        }
        private RelayCommand? pickAlphabetPathCommand;
        public RelayCommand PickAlphabetPathCommand
        {
            get
            {
                return pickAlphabetPathCommand ??
                    (pickAlphabetPathCommand = new RelayCommand(obj =>
                    {
                        OpenFileDialog openFileDialog = new();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            AlphabetFilePath = openFileDialog.FileName;
                        }
                    }));
            }
        }
        private RelayCommand? pickTextPathCommand;
        public RelayCommand PickTextPathCommand
        {
            get
            {
                return pickTextPathCommand ??
                    (pickTextPathCommand = new RelayCommand(obj =>
                    {
                        OpenFileDialog openFileDialog = new();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            TextFilePath = openFileDialog.FileName;
                        }
                    }));
            }
        }
        private RelayCommand? pickResultPathCommand;

        public RelayCommand PickResultPathCommand
        {
            get
            {
                return pickResultPathCommand ??
                    (pickResultPathCommand = new RelayCommand(obj =>
                    {
                        OpenFileDialog openFileDialog = new();
                        if (openFileDialog.ShowDialog() == true)
                        {
                            ResultFilePath = openFileDialog.FileName;
                        }
                    }));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler? PathChanged;
        public void OnPathChanged([CallerMemberName] string prop = "")
        {
            PathChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
