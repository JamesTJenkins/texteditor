using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using DiffMatchPatch;

namespace texteditor.Models {
	public struct TextChange {
		public int position;
		public string text;
		public bool isInsertion;
		public int caretPosition;
	}

	public partial class Document : ObservableObject {
		[ObservableProperty] private string title = "untitled";
		[ObservableProperty] private string text = string.Empty;
		[ObservableProperty] private string path = string.Empty;
		[ObservableProperty] private bool isDirty = false;
		[ObservableProperty] private int caretPosition = 0;

		public int UndoStackCount => undoStack.Count;
		public int RedoStackCount => redoStack.Count;

		private Stack<TextChange> undoStack = new();
		private Stack<TextChange> redoStack = new();
		private bool processingUndoRedo = false;

		partial void OnTextChanged(string? oldValue, string newValue) {
			if (processingUndoRedo || oldValue == newValue)
				return;

			List<TextChange> diff = ComputeDiff(oldValue ?? "", newValue);
			foreach (TextChange change in diff)
				undoStack.Push(change);
			redoStack.Clear();

			IsDirty = true;
			if (!Title.EndsWith("*"))
				Title += "*";
		}

		public void ResetDirty() {
			IsDirty = false;
			Title = Title.TrimEnd('*');
		}

		public void Undo() {
			if (undoStack.Count == 0)
				return;

			TextChange change = undoStack.Pop();
			redoStack.Push(change);
			ApplyChange(change, true);
		}

		public void Redo() {
			if (redoStack.Count == 0)
				return;

			TextChange change = redoStack.Pop();
			undoStack.Push(change);
			ApplyChange(change, false);
		}

		private void ApplyChange(TextChange change, bool reverse) {
			processingUndoRedo = true;
			bool isInsertion = reverse ? !change.isInsertion : change.isInsertion;

			if (isInsertion) {
				Text = Text.Insert(change.position, change.text);
			} else {
				Text = Text.Remove(change.position, change.text.Length);
			}

			CaretPosition = change.caretPosition;
			processingUndoRedo = false;
		}

		private List<TextChange> ComputeDiff(string oldText, string newText) {
			diff_match_patch dmp = new();
			List<Diff> diffs = dmp.diff_main(oldText, newText);
			dmp.diff_cleanupSemantic(diffs);

			List<TextChange> changes = new();
			int position = 0;

			foreach (var diff in diffs) {
				switch (diff.operation) {
					case Operation.DELETE:
					changes.Add(new TextChange {
						position = position,
						text = diff.text,
						isInsertion = false,
						caretPosition = position
					});
					break;
					case Operation.INSERT:
					changes.Add(new TextChange {
						position = position,
						text = diff.text,
						isInsertion = true,
						caretPosition = position + diff.text.Length
					});
					break;
					case Operation.EQUAL:
					position += diff.text.Length;
					break;
				}
			}

			return changes;
		}
	}
}