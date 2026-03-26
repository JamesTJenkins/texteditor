using System;
using System.Windows.Input;
using System.IO;
using Avalonia.Platform.Storage;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using texteditor.Models;

namespace texteditor.ViewModels {
	public partial class MainWindowViewModel : ViewModelBase {
		[ObservableProperty] private ObservableCollection<Document> documents = new();
		[ObservableProperty] private int selectedDocumentIndex = -1;
		[ObservableProperty] private Window window;

		public RelayCommand NewDocumentCommand { get; }
		public RelayCommand OpenDocumentCommand { get; }
		public RelayCommand CloseDocumentCommand { get; }
		public RelayCommand SaveDocumentCommand { get; }
		public RelayCommand SaveAsDocumentCommand { get; }

		public MainWindowViewModel(Window window) {
			this.window = window;
			NewDocumentCommand = new RelayCommand(_ => NewDocument());
			OpenDocumentCommand = new RelayCommand(_ => OpenDocument());
			SaveDocumentCommand = new RelayCommand(_ => SaveDocument(), _ => SelectedDocumentIndex >= 0);
			SaveAsDocumentCommand = new RelayCommand(_ => SaveAsDocument(), _ => SelectedDocumentIndex >= 0);
			CloseDocumentCommand = new RelayCommand(_ => CloseDocument(), _ => SelectedDocumentIndex >= 0);
		
			NewDocument();
		}

		partial void OnSelectedDocumentIndexChanged(int value) {
			SaveDocumentCommand.RaiseCanExecuteChanged();
			SaveAsDocumentCommand.RaiseCanExecuteChanged();
			CloseDocumentCommand.RaiseCanExecuteChanged();
		}

		private void NewDocument() {
			Documents.Add(new Document());
			SelectedDocumentIndex = Documents.Count - 1;
		}

		private async void OpenDocument() {
			var options = new FilePickerOpenOptions {
				Title = "Load File",
				FileTypeFilter = new[] {
					new FilePickerFileType("Text File") {
						Patterns = new[] { "*.txt" }
					}
				}
			};

			IReadOnlyList<IStorageFile> result = await Window.StorageProvider.OpenFilePickerAsync(options);
			foreach (IStorageFile file in result) {
				Document newDoc = new() {
					Title = file.Name,
					Text = await File.ReadAllTextAsync(result[0].Path.AbsolutePath)
				};
				newDoc.ResetDirty();
				Documents.Add(newDoc);
				SelectedDocumentIndex = Documents.Count - 1;
			}
		}

		private async void SaveDocument() {
			string path = Documents[SelectedDocumentIndex].Path;
			if (path == string.Empty || Path.Exists(path)) {
				SaveAsDocument();
				return;
			}

			FilePickerSaveOptions options = new FilePickerSaveOptions {
				Title = "Save File",
				FileTypeChoices = new[] {
					new FilePickerFileType("Text File") {
						Patterns = new[] { "*.txt" }
					}
				}
			};

			IStorageFile? result = await Window.StorageProvider.SaveFilePickerAsync(options);
			if (result != null) {
				await File.WriteAllTextAsync(result.Path.LocalPath, Documents[SelectedDocumentIndex].Text);
				Documents[SelectedDocumentIndex].ResetDirty();
			}
		}

		private async void SaveAsDocument() {
			FilePickerSaveOptions options = new FilePickerSaveOptions {
				Title = "Save As File",
				FileTypeChoices = new[] {
					new FilePickerFileType("Text File") {
						Patterns = new[] { "*.txt" }
					}
				}
			};

			IStorageFile? result = await Window.StorageProvider.SaveFilePickerAsync(options);
			if (result != null) {
				Documents[SelectedDocumentIndex].Title = result.Name;
				await File.WriteAllTextAsync(result.Path.LocalPath, Documents[SelectedDocumentIndex].Text);
				Documents[SelectedDocumentIndex].ResetDirty();
			}
		}

		private void CloseDocument() {
			Documents.RemoveAt(SelectedDocumentIndex);
			SelectedDocumentIndex = Math.Max(0, Math.Min(SelectedDocumentIndex, Documents.Count - 1));
		
			if (Documents.Count == 0)
				SelectedDocumentIndex = -1;
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

		public void RaiseCanExecuteChanged() {
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}