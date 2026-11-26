using System.Reflection;
using Microsoft.Win32;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class CommonItemDialogTests
{
	[Fact]
	public void File_and_folder_dialogs_share_CommonItemDialog()
	{
		Assert.Equal(typeof(CommonItemDialog), typeof(FileDialog).BaseType);
		Assert.Equal(typeof(CommonItemDialog), typeof(OpenFolderDialog).BaseType);
		Assert.Equal(typeof(CommonDialog), typeof(CommonItemDialog).BaseType);
		Assert.True(typeof(CommonItemDialog).IsAbstract);
	}

	[Fact]
	public void Item_properties_live_on_the_base_and_reset_together()
	{
		var dialog = new OpenFolderDialog { Title = "t", InitialDirectory = "/x", ShowHiddenItems = true };
		Assert.True(dialog.ValidateNames);
		Assert.Empty(dialog.CustomPlaces);
		dialog.Reset();
		Assert.Equal("", dialog.Title);
		Assert.Equal("", dialog.InitialDirectory);
		Assert.False(dialog.ShowHiddenItems);
		Assert.Equal(typeof(CommonItemDialog), typeof(OpenFolderDialog).GetProperty("Title")!.DeclaringType);
	}

	[Fact]
	public void SafeFolderName_is_the_last_segment()
	{
		var dialog = new OpenFolderDialog { FolderName = "/a/b" };
		Assert.Equal("b", dialog.SafeFolderName);
		Assert.Equal(new[] { "b" }, dialog.SafeFolderNames);
		dialog.FolderName = "/a/b/";
		Assert.Equal("b", dialog.SafeFolderName);
		Assert.Equal("", new OpenFolderDialog().SafeFolderName);
	}

	[Fact]
	public void ShowDialog_is_virtual()
	{
		var method = typeof(CommonDialog).GetMethod("ShowDialog", BindingFlags.Public | BindingFlags.Instance,
			null, System.Type.EmptyTypes, null)!;
		Assert.True(method.IsVirtual);
	}

	[Fact]
	public void AddToRecent_is_on_every_item_dialog_and_defaults_true()
	{
		CommonItemDialog[] dialogs = { new OpenFileDialog(), new SaveFileDialog(), new OpenFolderDialog() };
		foreach (var dialog in dialogs)
		{
			Assert.True(dialog.AddToRecent);
			dialog.AddToRecent = false;
			dialog.Reset();
			Assert.True(dialog.AddToRecent);
		}
		Assert.Equal(typeof(CommonItemDialog), typeof(CommonItemDialog).GetProperty("AddToRecent")!.DeclaringType);
	}
}
