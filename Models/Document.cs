using CommunityToolkit.Mvvm.ComponentModel;

namespace texteditor.Models {
	public partial class Document : ObservableObject {
		[ObservableProperty] private string title = "untitled";
		[ObservableProperty] private string text = string.Empty;
		[ObservableProperty] private string path = string.Empty;
	}
}