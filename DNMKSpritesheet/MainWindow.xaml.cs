using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Image = System.Drawing.Image;

namespace DNMKSpritesheet
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{
		/// <summary>
		/// Do not access this. Use CurrentFile instead.
		/// </summary>
		private string current_file;

		private bool animation_shot_type_selected;
		private bool unsaved;

		private static int MAX_RECENT_FILES = 15;
		
		private static string app_data_path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\DNMKSpritesheet";
		private static string recent_files_path = app_data_path + "\\recent.txt";

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private bool Unsaved
		{
			get => unsaved;
			set
			{
				unsaved = value;
				UpdateTitle();
			}
		}

		private string CurrentFile
		{
			get => current_file;
			set
			{
				current_file = value;
				OnPropertyChanged("HasCurrentFile");
			}
		}

		#region BindingProperties

		public bool AnimationShotTypeSelected
		{
			get
			{
				if (!HasShotTypeSelected)
				{
					return false;
				}

				return shot_manager.ShotTypes[LstShots.SelectedIndex] is AnimationShotType;
			}
		}

		public bool HasCurrentFile => CurrentFile != null;

		public bool HasSpriteSelected => LstSprite.SelectedIndex != -1;

		public bool HasShotTypeSelected => LstShots.SelectedIndex != -1;

		public bool HasAnimationFrameSelected => LstShotSprites.SelectedIndex != -1;

		#endregion

		private ShotManager shot_manager;

		private ShotType SelectedShotType
		{
			get
			{
				if (LstShots.SelectedIndex == -1)
				{
					return null;
				}

				return shot_manager.ShotTypes[LstShots.SelectedIndex];
			}
		}

		private Sprite SelectedSprite
		{
			get
			{
				if (LstSprite.SelectedIndex == -1)
				{
					return null;
				}

				return shot_manager.Sprites[LstSprite.SelectedIndex];
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			if (!Directory.Exists(app_data_path))
			{
				Directory.CreateDirectory(app_data_path);
			}
			
			shot_manager = new ShotManager("dasd");
			InitNewSpriteSheet();
			UpdateShotTypeData();

			CurrentFile = null;

			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i <= args.Length - 1; i++)
			{
				if (args[i].EndsWith(".sprt"))
				{
					OpenFile(args[i]);
					break;
				}
			}

			OnPropertyChanged("HasShotTypeSelected");
			OnPropertyChanged("HasSpriteSelected");
			OnPropertyChanged("AnimationShotTypeSelected");
			OnPropertyChanged("HasAnimationFrameSelected");

			if (File.Exists(recent_files_path))
			{
				//Add any still existing recent files to this, then write it all to the recent files text
				//Serves to remove any nonexistent files
				//Also removes files past 15
				string new_recent_files = "";
				
				using (StreamReader reader = new StreamReader(recent_files_path))
				{
					int i = 0;
					
					while (reader.Peek() != -1 && i < MAX_RECENT_FILES)
					{
						string file = reader.ReadLine();

						if (File.Exists(file))
						{
							new_recent_files += file + "\n";
							
							MenuItem recent_item = new MenuItem();
							recent_item.Tag = file;
							recent_item.Header = Path.GetFileNameWithoutExtension(file);
							recent_item.PreviewMouseDown += MnuRecentFile_OnPreviewClick;

							MnuRecent.Items.Insert(0, recent_item);

							i++;
						}
					}
				}
				
				File.WriteAllText(recent_files_path, new_recent_files);
			}
			else
			{
				File.WriteAllText(recent_files_path, "");
			}
		}

		private void MnuRecentFile_OnPreviewClick(object sender, MouseButtonEventArgs e)
		{
			string filename = (string) ((MenuItem) sender).Tag;

			if (!File.Exists(filename))
			{
				MessageBox.Show("File not found.");
				//TODO: Remove file from recent
				return;
			}
			
			OpenFile(filename);
		}

		private void UpdateTitle()
		{
			if (CurrentFile == null)
			{
				MWindow.Title = "DNMK Spritesheet";
			}
			else
			{
				string text = "DNMK Spritesheet - " + Path.GetFileName(CurrentFile);

				if (Unsaved)
				{
					text += " *";
				}

				MWindow.Title = text;
			}
		}

		private void LstSprite_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (LstSprite.SelectedIndex == -1)
			{
				return;
			}

			if (e.Key == Key.Delete)
			{
				DeleteSpriteConfirm(shot_manager.Sprites[LstSprite.SelectedIndex].Name);
			}
			else if(e.Key == Key.F2)
			{
				if (SelectedSprite == null)
				{
					return;
				}
				
				try
				{
					RenameWindow rename_window = new RenameWindow(SelectedSprite.Name);

					rename_window.ShowDialog();

					if (rename_window.ReturnValue != null)
					{
						shot_manager.RenameSprite(SelectedSprite.Name, rename_window.ReturnValue);
					}
				}
				catch (ArgumentException ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		private bool DeleteSpriteConfirm(string name)
		{
			MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?", "Delete",
				MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes)
			{
				shot_manager.RemoveSprite(name);
				Unsaved = true;
				return true;
			}

			return false;
		}

		private bool DeleteShotTypeConfirm(string name)
		{
			MessageBoxResult result = MessageBox.Show("Are you sure you want to delete " + name + "?", "Delete",
				MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes)
			{
				shot_manager.RemoveShotType(name);
				Unsaved = true;
				return true;
			}

			return false;
		}

		private void LstShotSprites_OnKeyDown(object sender, KeyEventArgs e)
		{
			int index = LstShotSprites.SelectedIndex;

			if (index == -1)
			{
				return;
			}

			if (e.Key == Key.Delete)
			{
				((AnimationShotType) SelectedShotType).Animation.RemoveAnimationFrame(index);
				Unsaved = true;
			}
			else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				//Duplicate selected frame
				AnimationShotType shot_type = (AnimationShotType) SelectedShotType;
				Sprite sprite = shot_type.Animation.GetSpriteAtIndex(index);
				int delay = shot_type.Animation.GetDelayAtIndex(index);
				shot_type.Animation.InsertAnimationFrame(index + 1, delay, sprite);
				Unsaved = true;
			}
		}

		private void UpdateShotTypeData()
		{
			if (HasShotTypeSelected)
			{
				ClpDelayColor.SelectedColor = SelectedShotType.DelayColor;
				TxtCollision.Text = SelectedShotType.Collision.ToString();
			}
		}

		private void LstShots_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = LstShots.SelectedIndex;

			if (SelectedShotType is AnimationShotType anim_shot)
			{
				LstShotSprites.ItemsSource = anim_shot.Animation.AnimationFrames;
			}
			else
			{
				LstShotSprites.ItemsSource = null;
			}

			UpdateShotTypeData();
			OnPropertyChanged("HasShotTypeSelected");
			OnPropertyChanged("AnimationShotTypeSelected");
		}

		private void BtnShotTypeSave_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (SelectedShotType == null)
			{
				return;
			}

			try
			{
				int collision;

				try
				{
					collision = Convert.ToInt32(TxtCollision.Text);
				}
				catch (Exception ex) when (ex is FormatException || ex is OverflowException)
				{
					throw new ArgumentException("Invalid number format.");
				}

				if (collision < 0)
				{
					throw new ArgumentException("Collision size cannot be less than zero.");
				}

				SelectedShotType.Collision = collision;

				Color? color = ClpDelayColor.SelectedColor;

				if (color == null)
				{
					color = Colors.Transparent;
				}

				SelectedShotType.DelayColor = (Color) color;

				Unsaved = true;
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void MnuSpriteDelete_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (LstSprite.SelectedIndex == -1)
			{
				MessageBox.Show("No sprite selected.");
				return;
			}

			DeleteSpriteConfirm(shot_manager.Sprites[LstSprite.SelectedIndex].Name);
		}

		private void MnuMakeStillShot_OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (LstSprite.SelectedIndex == -1)
			{
				MessageBox.Show("No sprite selected.");
				return;
			}

			int index = LstSprite.SelectedIndex;

			CreateStillShotTypeWindow create_shot_type_window =
				new CreateStillShotTypeWindow(shot_manager.Sprites[index]);
			create_shot_type_window.ShowDialog();
			create_shot_type_window.Focus();

			ShotType return_value = create_shot_type_window.ReturnValue;

			if (return_value != null)
			{
				try
				{
					shot_manager.AddShotType(return_value);

					Unsaved = true;
				}
				catch (ArgumentException ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		private void LstShots_OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Delete)
			{
				if (LstShots.SelectedIndex == -1)
				{
					return;
				}

				DeleteShotTypeConfirm(shot_manager.ShotTypes[LstShots.SelectedIndex].Name);
			}
			else if(e.Key == Key.F2)
			{
				if (SelectedShotType == null)
				{
					return;
				}
				
				try
				{
					RenameWindow rename_window = new RenameWindow(SelectedShotType.Name);

					rename_window.ShowDialog();

					if (rename_window.ReturnValue != null)
					{
						shot_manager.RenameShotType(SelectedShotType.Name, rename_window.ReturnValue);
					}
				}
				catch (ArgumentException ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}

		private void BtnShotTypeRevert_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (SelectedShotType == null)
			{
				return;
			}

			UpdateShotTypeData();
		}

		private void LstSprite_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (LstSprite.SelectedIndex == -1)
			{
				return;
			}

			if (SelectedShotType != null && SelectedShotType is AnimationShotType anim_shot)
			{
				Sprite sprite = shot_manager.Sprites[LstSprite.SelectedIndex];

				shot_manager.AddAnimationFrame(anim_shot.Name, 2, sprite.Name);
				Unsaved = true;
			}
		}

		private void MnuMakeAnimShot_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (LstSprite.SelectedIndex == -1)
			{
				MessageBox.Show("No sprite selected.");
				return;
			}

			int index = LstSprite.SelectedIndex;

			CreateAnimShotTypeWindow create_shot_type_window =
				new CreateAnimShotTypeWindow(shot_manager.Sprites[index]);
			create_shot_type_window.ShowDialog();
			create_shot_type_window.Focus();

			ShotType return_value = create_shot_type_window.ReturnValue;

			if (return_value != null)
			{
				shot_manager.AddShotType(return_value);

				Unsaved = true;
			}
		}

		private void MnuMakeAnimationShot_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			CreateAnimShotTypeWindow create_shot_type_window = new CreateAnimShotTypeWindow(null);
			create_shot_type_window.ShowDialog();
			create_shot_type_window.Focus();

			ShotType return_value = create_shot_type_window.ReturnValue;

			if (return_value != null)
			{
				shot_manager.AddShotType(return_value);

				Unsaved = true;
			}
		}

		private void LstShotSprites_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int index = LstShotSprites.SelectedIndex;

			if (HasAnimationFrameSelected)
			{
				TxtDelay.Text = ((AnimationShotType) SelectedShotType).Animation.AnimationFrames[index].Delay
					.ToString();
			}

			OnPropertyChanged("HasAnimationFrameSelected");
		}

		private void BtnDelaySave_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			int index = LstShotSprites.SelectedIndex;

			if (index == -1)
			{
				return;
			}

			try
			{
				int delay;

				try
				{
					delay = Convert.ToInt32(TxtDelay.Text);
				}
				catch (Exception ex) when (ex is FormatException || ex is OverflowException)
				{
					throw new ArgumentException("Invalid number format.");
				}

				shot_manager.SetAnimationFrameDelay(SelectedShotType.Name, index, delay);

				Unsaved = true;
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		private void MnuNewSprite_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			CreateSpriteWindow crt = new CreateSpriteWindow();
			crt.ShowDialog();
			crt.Focus();

			if (crt.ReturnValue != null)
			{
				shot_manager.AddSprite(crt.ReturnValue);

				Unsaved = true;
			}
		}

		private void MnuSaveAs_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			SaveFileDialog svfile = new SaveFileDialog();
			svfile.Filter = "Spritesheet files (*.sprt)|*.sprt|All files (*.*)|*.*";

			if (svfile.ShowDialog() == true)
			{
				try
				{
					shot_manager.Save(svfile.FileName);

					CurrentFile = svfile.FileName;
					Unsaved = false;
				}
				catch (ArgumentException ex)
				{
					MessageBox.Show(ex.Message);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					MessageBox.Show("Error saving file.");
					return;
				}
			}
		}

		private void InitNewSpriteSheet()
		{
			LstSprite.ItemsSource = shot_manager.Sprites;
			LstShots.ItemsSource = shot_manager.ShotTypes;
			LstShotSprites.ItemsSource = null;
		}

		private void MnuOpenFile_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog opfile = new OpenFileDialog();
			opfile.Filter = "Spritesheet files (*.sprt)|*.sprt|All files (*.*)|*.*";
			opfile.Multiselect = false;

			if (opfile.ShowDialog() == true)
			{
				try
				{
					OpenFile(opfile.FileName);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					MessageBox.Show("Error opening file.");
					return;
				}
			}
		}

		private void OpenFile(string filename)
		{
			shot_manager = ShotManager.FromFile(filename);
			InitNewSpriteSheet();

			CurrentFile = filename;
			Unsaved = false;
			
			AddFileToRecent(filename);
		}

		private void AddFileToRecent(string filename)
		{
			//Add to recent files file

			string recent_text = File.ReadAllText(recent_files_path);
			string[] recent_files_arr = recent_text.Split('\n');
			List<String> recent_files = recent_files_arr.ToList();
			
			//Remove the last element because it will just be empty.
			recent_files.RemoveAt(recent_files.Count - 1);
				
			bool found = false;
			for (int i = 0; i < recent_files.Count; i++)
			{
				if (recent_files[i].Equals(filename))
				{
					found = true;
					
					recent_files.RemoveAt(i);
					
					break;
				}
			}
			
			recent_files.Add(filename);

			if (recent_files.Count > MAX_RECENT_FILES)
			{
				recent_files.RemoveAt(0);
			}

			string new_text = "";

			foreach (string recent_file in recent_files)
			{
				new_text += recent_file + "\n";
			}
			
			File.WriteAllText(recent_files_path, new_text);

			//Refresh menu items

			MnuRecent.Items.Clear();

			foreach (string recent_file in recent_files)
			{
				MenuItem recent_item = new MenuItem();
				recent_item.Tag = recent_file;
				recent_item.Header = Path.GetFileNameWithoutExtension(recent_file);
				recent_item.PreviewMouseDown += MnuRecentFile_OnPreviewClick;

				MnuRecent.Items.Insert(0, recent_item);
			}
		}

		private void MnuSave_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (CurrentFile == null)
			{
				return;
			}

			try
			{
				shot_manager.Save(CurrentFile);
				Unsaved = false;
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show(ex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				MessageBox.Show("Error saving file.");
				return;
			}
		}

		private void MnuNew_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			MessageBoxResult result = MessageBox.Show("Are you sure? All unsaved progress will be lost.", "New",
				MessageBoxButton.YesNo);

			if (result == MessageBoxResult.Yes)
			{
				shot_manager = new ShotManager("NewCustomShot");
				InitNewSpriteSheet();
				CurrentFile = null;
				Unsaved = false;
			}
		}

		private void MnuImportSprites_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			OpenFileDialog opfile = new OpenFileDialog();
			opfile.Multiselect = true;
			opfile.Filter = "Image files (*.png;*.jpg)|*.png;*.jpg|All files (*.*)|*.*";

			if (opfile.ShowDialog() == true)
			{
				Bitmap[] images = new Bitmap[opfile.FileNames.Length];

				int i = 0;
				foreach (string filename in opfile.FileNames)
				{
					try
					{
						images[i] = (Bitmap) Image.FromFile(filename);
					}
					catch (Exception exception)
					{
						Console.WriteLine(exception);
						MessageBox.Show("Error reading one or more files.");
						return;
					}

					i++;
				}

				Sprite[] sprites = new Sprite[opfile.FileNames.Length];

				i = 0;
				foreach (Bitmap image in images)
				{
					string name = Path.GetFileNameWithoutExtension(opfile.FileNames[i]);

					try
					{
						sprites[i] = new Sprite(image, name);
					}
					catch (ArgumentException ex)
					{
						MessageBox.Show(ex.Message);
						return;
					}

					i++;
				}

				foreach (Sprite sprite in sprites)
				{
					try
					{
						shot_manager.AddSprite(sprite);
						Unsaved = true;
					}
					catch (ArgumentException)
					{
						continue;
					}
				}
			}
		}

		private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				if (e.Key == Key.S)
				{
					if (CurrentFile == null)
					{
						return;
					}

					try
					{
						shot_manager.Save(CurrentFile);

						Unsaved = false;
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						MessageBox.Show("Error saving file.");
						return;
					}
				}
				else if (e.Key == Key.E)
				{
					QuickExport();
				}
			}
		}

		private void MnuExport_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (shot_manager.DelaySprite == null)
			{
				MessageBox.Show("Select a delay sprite before exporting.");
				return;
			}

			SaveFileDialog save_file_dialog = new SaveFileDialog();
			save_file_dialog.Filter = "Zip archive file (*.zip)|*.zip|All files (*.*)|*.*";

			if (save_file_dialog.ShowDialog() == true)
			{
				string temp_path = Path.GetTempPath() + "\\dnmkspr\\export";

				if (Directory.Exists(temp_path))
				{
					Directory.Delete(temp_path, true);
				}

				Directory.CreateDirectory(temp_path);

				string[] filenames = shot_manager.Export(temp_path);

				if (File.Exists(save_file_dialog.FileName))
				{
					File.Delete(save_file_dialog.FileName);
				}

				using (ZipArchive archive = ZipFile.Open(save_file_dialog.FileName, ZipArchiveMode.Create))
				{
					foreach (string filename in filenames)
					{
						archive.CreateEntryFromFile(temp_path + "\\" + filename, filename);
					}
				}

				Directory.Delete(temp_path, true);
			}
		}

		private void MnuSetDelaySprite_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			Sprite sprite = shot_manager.Sprites[LstSprite.SelectedIndex];
			shot_manager.DelaySprite = sprite;

			Unsaved = true;
		}

		private void MnuQuickExportOptions_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var options_window = new QuickExportOptionsWindow(shot_manager.QuickExportLocation);

			options_window.ShowDialog();

			if (options_window.ReturnValue != null)
			{
				shot_manager.QuickExportLocation = options_window.ReturnValue;
			}
		}

		private void MnuQuickExport_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			QuickExport();
		}

		private void QuickExport()
		{
			string location = shot_manager.QuickExportLocation;

			if (location.Equals("self"))
			{
				location = Path.GetDirectoryName(CurrentFile);
			}

			shot_manager.Export(location);
		}

		private void BtnAnimationFrameUp_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			AnimationShotType shot_type = (AnimationShotType) shot_manager.ShotTypes[LstShots.SelectedIndex];
			LstShotSprites.SelectedIndex = shot_type.MoveAnimationFrameUp(LstShotSprites.SelectedIndex);

			Unsaved = true;
		}

		private void BtnAnimationFrameDown_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			AnimationShotType shot_type = (AnimationShotType) shot_manager.ShotTypes[LstShots.SelectedIndex];
			LstShotSprites.SelectedIndex = shot_type.MoveAnimationFrameDown(LstShotSprites.SelectedIndex);

			Unsaved = true;
		}
	}
}