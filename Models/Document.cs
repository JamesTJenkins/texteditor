using CommunityToolkit.Mvvm.ComponentModel;

namespace texteditor.Models {
	public partial class Document : ObservableObject {
		[ObservableProperty] private string title = "untitled";
		[ObservableProperty] private string text = string.Empty;
		[ObservableProperty] private string path = string.Empty;
		[ObservableProperty] private bool isDirty = false;

		partial void OnTextChanged(string value) {
			IsDirty = true;
			if (!Title.EndsWith("*"))
				Title += "*";
		}

		public void ResetDirty() {
			IsDirty = false;
			Title = Title.TrimEnd('*');
		}
	}
}