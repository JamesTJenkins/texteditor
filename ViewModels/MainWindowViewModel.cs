using System;
using System.Windows.Input;
using System.IO;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Collections.Generic;

namespace texteditor.ViewModels {
	public partial class MainWindowViewModel : ViewModelBase {
		private readonly TextEditorModel model = new();
		private readonly Window window;

		public string Text {
			get => model.Text;
			set {
				model.Text = value;
				OnPropertyChanged();
			}
		}

		public ICommand SaveCommand { get; }
		public ICommand LoadCommand { get; }

		public MainWindowViewModel(Window window) {
			this.window = window;
			SaveCommand = new RelayCommand(_ => SaveFile());
			LoadCommand = new RelayCommand(_ => LoadFile());
		}

		private async void SaveFile() {
			FilePickerSaveOptions options = new FilePickerSaveOptions {
				Title = "Save File",
				FileTypeChoices = new[] {
					new FilePickerFileType("Text File") {
						Patterns = new[] { "*.txt" }
					}
				}
			};

			IStorageFile? result = await window.StorageProvider.SaveFilePickerAsync(options);
			if (result != null)
				await File.WriteAllTextAsync(result.Path.LocalPath, Text);
		}

		private async void LoadFile() {
			var options = new FilePickerOpenOptions {
				Title = "Load File",
				FileTypeFilter = new[] {
					new FilePickerFileType("Text File") {
						Patterns = new[] { "*.txt" }
					}
				}
			};

			IReadOnlyList<IStorageFile> result = await window.StorageProvider.OpenFilePickerAsync(options);
			if (result.Count > 0)
				Text = await File.ReadAllTextAsync(result[0].Path.AbsolutePath);
		}
	}

	public class RelayCommand : ICommand {
		private readonly Action<object?> execute;
		private readonly Predicate<object?>? canExecute;

		public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null) {
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public bool CanExecute(object? param) => canExecute?.Invoke(param) ?? true;
		public void Execute(object? param) => execute(param);

		public event EventHandler? CanExecuteChanged;
	}
}