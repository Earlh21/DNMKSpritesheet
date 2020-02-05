using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Drawing.Size;

namespace DNMKSpritesheet
{
	public class ShotManager
	{
		private static List<Type> KnownTypes { get; set; }

		public ObservableCollection<Sprite> Sprites { get; private set; }
		public ObservableCollection<ShotType> ShotTypes { get; private set; }
		public Sprite DelaySprite { get; set; }
		public string Name { get; set; }
		public string QuickExportLocation { get; set; }

		static ShotManager()
		{
			KnownTypes = new List<Type>();
			KnownTypes.Add(typeof(Sprite));
			KnownTypes.Add(typeof(StillShotType));
			KnownTypes.Add(typeof(AnimationShotType));
			KnownTypes.Add(typeof(BitmapImage));
			KnownTypes.Add(typeof(Animation));
			KnownTypes.Add(typeof(AnimationFrame));
			KnownTypes.Add(typeof(MemoryStream));
		}

		public ShotManager(string name)
		{
			Sprites = new ObservableCollection<Sprite>();
			ShotTypes = new ObservableCollection<ShotType>();
			Name = name;
			QuickExportLocation = "self";
		}

		private Sprite GetSpriteByName(string name)
		{
			foreach (Sprite sprite in Sprites)
			{
				if (sprite.Name.Equals(name))
				{
					return sprite;
				}
			}

			return null;
		}

		private ShotType GetShotTypeByName(string shot_name)
		{
			foreach (ShotType shot in ShotTypes)
			{
				if (shot.Name.Equals(shot_name))
				{
					return shot;
				}
			}

			return null;
		}

		private int GetSpriteIndexByName(string name)
		{
			int index = 0;

			foreach (Sprite sprite in Sprites)
			{
				if (sprite.Name.Equals(name))
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		private int GetShotTypeIndexByName(string name)
		{
			int i = 0;
			foreach (ShotType shot_type in ShotTypes)
			{
				if (shot_type.Name.Equals(name))
				{
					return i;
				}

				i++;
			}

			return -1;
		}

		private void RemoveShotTypeAt(int index)
		{
			ShotTypes.RemoveAt(index);
		}

		public void RemoveSprite(string name)
		{
			Sprite sprite = GetSpriteByName(name);

			if (sprite == null)
			{
				throw new ArgumentException("Sprite not found.");
			}

			for (int i = 0; i < ShotTypes.Count; i++)
			{
				ShotType shot_type = ShotTypes[i];

				if (shot_type is StillShotType still)
				{
					if (still.Sprite == sprite)
					{
						RemoveShotType(still.Name);
						i--;
					}
				}
				else if (shot_type is AnimationShotType anim)
				{
					anim.RemoveSprite(sprite);
					if (anim.Animation.Length == 0)
					{
						RemoveShotType(anim.Name);
						i--;
					}
				}
			}

			Sprites.Remove(sprite);

			if (DelaySprite == sprite)
			{
				DelaySprite = null;
			}
		}

		public void ChangeSprite(string name, Bitmap new_sprite)
		{
			Sprite sprite = GetSpriteByName(name);

			if (sprite == null)
			{
				throw new ArgumentException("Sprite name not found.");
			}

			sprite.Image = new_sprite;
		}

		public void RenameShotType(string from, string to)
		{
			ShotType shot_type = GetShotTypeByName(from);

			if (shot_type == null)
			{
				throw new ArgumentException("Shot type name not found.");
			}

			foreach (ShotType st in ShotTypes)
			{
				if (st.Name.Equals(to))
				{
					throw new ArgumentException("Cannot change name to " + to + ", as a duplicate was found.");
				}
			}
			
			shot_type.Name = to;
		}
		
		public void RenameSprite(string from, string to)
		{
			Sprite sprite = GetSpriteByName(from);

			if (sprite == null)
			{
				throw new ArgumentException("Sprite name not found.");
			}

			foreach (Sprite sp in Sprites)
			{
				if (sp.Name.Equals(to))
				{
					throw new ArgumentException("Cannot change name to " + to + ", as a duplicate was found.");
				}
			}

			sprite.Name = to;
			foreach (ShotType shot in ShotTypes)
			{
				if (shot.Name.Equals(from))
				{
					shot.Name = to;
				}
			}
		}

		public void AddStillShotType(string name, string sprite_name, int collision, Color delay_color)
		{
			if (ContainsShotName(name))
			{
				throw new ArgumentException("Shot to be added has a duplicate name.");
			}

			ShotTypes.Add(new StillShotType(name, GetSpriteByName(sprite_name), collision, delay_color));
		}

		public void AddAnimationShotType(string name, int collision, Color delay_color)
		{
			if (ContainsShotName(name))
			{
				throw new ArgumentException("Shot to be added has a duplicate name.");
			}

			ShotTypes.Add(new AnimationShotType(name, collision, delay_color));
		}

		public void AddShotType(ShotType shot_type)
		{
			if (ContainsShotName(shot_type.Name))
			{
				throw new ArgumentException("Shot to be added has a duplicate name.");
			}

			ShotTypes.Add(shot_type);
		}

		public void RemoveShotType(string name)
		{
			RemoveShotTypeAt(GetShotTypeIndexByName(name));
		}

		public void SetAnimationFrameDelay(string shot_name, int animation_index, int delay)
		{
			ShotType shot_type = GetShotTypeByName(shot_name);

			if (shot_type == null)
			{
				throw new ArgumentException("Shot Type not found.");
			}

			if (shot_type is AnimationShotType anim_shot)
			{
				anim_shot.Animation.AnimationFrames[animation_index].Delay = delay;
			}
			else
			{
				throw new ArgumentException("Specified Shot Type is not an animation.");
			}
		}

		public void AddAnimationFrame(string shot_name, int delay, string sprite_name)
		{
			ShotType shot_type = GetShotTypeByName(shot_name);

			if (shot_type == null)
			{
				throw new ArgumentException("Shot Type not found.");
			}

			if (shot_type is AnimationShotType anim_shot)
			{
				if (anim_shot.Animation.Length == 0)
				{
					int index = GetShotTypeIndexByName(shot_name);
				}

				anim_shot.Animation.AddAnimationFrame(delay, GetSpriteByName(sprite_name));
			}
			else
			{
				throw new ArgumentException("Specified Shot Type is not an animation.");
			}
		}

		public void RemoveAnimationData(string shot_name, int index)
		{
			ShotType shot_type = GetShotTypeByName(shot_name);

			if (shot_type == null)
			{
				throw new ArgumentException("Shot not found.");
			}

			if (shot_type is AnimationShotType a_shot)
			{
				a_shot.Animation.RemoveAnimationFrame(index);
			}
			else
			{
				throw new ArgumentException("Specified shot is not an animation.");
			}
		}

		/// <summary>
		/// Checks whether the SpriteSheet contains the specified sprite or not.
		/// </summary>
		/// <param name="name">Name to check for</param>
		/// <returns>Whether or not the sprite exists</returns>
		public bool ContainsSpriteName(string name)
		{
			foreach (Sprite sprite in Sprites)
			{
				if (sprite.Name.Equals(name))
				{
					return true;
				}
			}

			return false;
		}

		public bool ContainsShotName(string name)
		{
			foreach (ShotType shot in ShotTypes)
			{
				if (shot.Name.Equals(name))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds a new sprite to the SpriteSheet and packs it.
		/// </summary>
		/// <param name="image">Image of the sprite to add</param>
		/// <param name="name">Name of the sprite to add</param>
		/// <exception cref="ArgumentException">If the name of the sprite to be added is a duplicate</exception>
		public void AddSprite(Bitmap image, string name)
		{
			foreach (Sprite sprite in Sprites)
			{
				if (sprite.Name.Equals(name))
				{
					throw new ArgumentException("Sprite to be added has a duplicate name.");
				}
			}

			Sprite new_sprite = new Sprite(image, name);
			Sprites.Add(new_sprite);
		}

		public void AddSprite(Sprite sprite)
		{
			foreach (Sprite sp in Sprites)
			{
				if (sp.Name.Equals(sprite.Name))
				{
					throw new ArgumentException("Sprite to be added has a duplicate name.");
				}
			}

			Sprites.Add(sprite);
		}

		/// <summary>
		/// Constructs the CustomShotData file.
		/// </summary>
		/// <returns>The CustomShotData file as text</returns>
		private string ConstructShotData(SpriteSheet sheet)
		{
			string shot_image = "shot_image = \"./" + Name + ".png\"";
			string to_return = shot_image + Environment.NewLine;
			
			if (DelaySprite != null)
			{
				Int32Rect ds_domain = sheet.GetSpriteDomain(DelaySprite);

				string delay_sprite_string = "delay_rect=(";
				delay_sprite_string += ds_domain.X + "," + ds_domain.Y + ",";
				delay_sprite_string += (ds_domain.X + ds_domain.Width).ToString() + ",";
				delay_sprite_string += (ds_domain.Y + ds_domain.Height).ToString() + ")";
				
				to_return += delay_sprite_string + Environment.NewLine;
			}

			int i = 1500;
			foreach (ShotType shot_type in ShotTypes)
			{
				DataString d_s = shot_type.GetDataString(sheet, i);
				string shot_string = d_s.ToString();

				to_return += shot_string + Environment.NewLine + Environment.NewLine;

				i++;
			}

			return to_return;
		}

		/// <summary>
		/// Constructs the CustomShotConst file.
		/// </summary>
		/// <returns>The CustomShotConst file as text</returns>
		private string ConstructShotConst(SpriteSheet sheet)
		{
			string to_return = "local {" + Environment.NewLine;
			to_return += "\tlet current = GetCurrentScriptDirectory();" + Environment.NewLine;
			to_return += "\tlet path = current ~ \"" + Name + "Data.dnh\";" + Environment.NewLine;
			to_return += "\tLoadEnemyShotData(path);" + Environment.NewLine;
			to_return += "}" + Environment.NewLine + Environment.NewLine;

			int i = 1500;
			foreach (ShotType shot_type in ShotTypes)
			{
				to_return += "let " + shot_type.Name + " = " + i + ";" + Environment.NewLine;

				i++;
			}

			return to_return;
		}

		public string[] Export(string folder)
		{
			if (ShotTypes.Count == 0)
			{
				throw new ArgumentException("Cannot export with no shot types.");
			}
			
			Directory.CreateDirectory(folder);

			SpriteSheet sheet = new SpriteSheet(Sprites.ToList());

			Bitmap image = sheet.Export();
			string shot_data = ConstructShotData(sheet);
			string shot_const = ConstructShotConst(sheet);

			string image_name = Name + ".png";
			string data_name = Name + "Data.dnh";
			string const_name = Name + "Const.dnh";

			string[] filenames = {image_name, data_name, const_name};

			image.Save(folder + "\\" + image_name);
			//File.CreateText(folder + "\\" + data_name).Close();
			File.WriteAllText(folder + "\\" + data_name, shot_data);
			//File.CreateText(folder + "\\" + const_name).Close();
			File.WriteAllText(folder + "\\" + const_name, shot_const);

			return filenames;
		}

		public void Save(string filename)
		{
			string temp_path = Path.GetTempPath() + "\\dnmkspr\\save";

			if (Directory.Exists(temp_path))
			{
				Directory.Delete(temp_path, true);
			}

			Directory.CreateDirectory(temp_path);
			using (StreamWriter writer = new StreamWriter(temp_path + "\\info.txt"))
			{
				if (DelaySprite == null)
				{
					writer.WriteLine();
				}
				else
				{
					writer.WriteLine(DelaySprite.Name);
				}

				writer.WriteLine(QuickExportLocation);
			}

			Directory.CreateDirectory(temp_path + "\\sprites");
			foreach (Sprite sprite in Sprites)
			{
				sprite.Image.Save(temp_path + "\\sprites\\" + sprite.Name + ".png");
			}

			Directory.CreateDirectory(temp_path + "\\shots");
			Directory.CreateDirectory(temp_path + "\\shots\\sprite_info");
			foreach (ShotType shot_type in ShotTypes)
			{
				string attribute_path = temp_path + "\\shots\\" + shot_type.Name + ".txt";
				string sprites_path = temp_path + "\\shots\\sprite_info\\" + shot_type.Name + "_sprites.txt";

				using (StreamWriter writer = new StreamWriter(attribute_path))
				{
					if (shot_type is AnimationShotType)
					{
						writer.WriteLine("Animation");
					}
					else if (shot_type is StillShotType)
					{
						writer.WriteLine("Still");
					}
					
					writer.WriteLine(shot_type.Name);
					writer.WriteLine(shot_type.Collision);
					Color c = shot_type.DelayColor;
					writer.WriteLine(c.R);
					writer.WriteLine(c.G);
					writer.WriteLine(c.B);
					writer.WriteLine(c.A);
				}

				if (shot_type is StillShotType still_shot_type)
				{
					using (StreamWriter writer = new StreamWriter(sprites_path))
					{
						writer.WriteLine(still_shot_type.Sprite.Name);
					}
				}
				else if (shot_type is AnimationShotType animation_shot_type)
				{
					using (StreamWriter writer = new StreamWriter(sprites_path))
					{
						foreach (AnimationFrame frame in animation_shot_type.Animation.AnimationFrames)
						{
							writer.WriteLine(frame.Sprite.Name);
							writer.WriteLine(frame.Delay);
						}
					}
				}
			}

			if (File.Exists(filename))
			{
				File.Delete(filename);
			}

			ZipFile.CreateFromDirectory(temp_path, filename);

			Directory.Delete(temp_path, true);
		}

		public static ShotManager FromFile(string filename)
		{
			//Set up temp folder
			string temp_path = Path.GetTempPath() + "\\dnmkspr\\load";

			if (Directory.Exists(temp_path))
			{
				Directory.Delete(temp_path, true);
			}

			Directory.CreateDirectory(temp_path);

			ZipFile.ExtractToDirectory(filename, temp_path);


			string[] sprite_files = Directory.GetFiles(temp_path + "\\sprites");
			string[] shot_type_files = Directory.GetFiles(temp_path + "\\shots");

			ShotManager manager = new ShotManager(Path.GetFileNameWithoutExtension(filename));

			//Read sprites
			foreach (string file in sprite_files)
			{
				Bitmap image;
				byte[] image_bytes = File.ReadAllBytes(file);

				using (MemoryStream ms = new MemoryStream(image_bytes))
				{
					image = (Bitmap) Image.FromStream(ms);
				}

				string name = Path.GetFileNameWithoutExtension(file);

				manager.AddSprite(image, name);
			}

			//Read shot types
			foreach (string file in shot_type_files)
			{
				string type;
				string name;
				int collision;
				Color delay_color;

				//Read common data
				using (StreamReader reader = new StreamReader(file))
				{
					type = reader.ReadLine();
					name = reader.ReadLine();
					collision = Convert.ToInt32(reader.ReadLine());
					byte r = Convert.ToByte(reader.ReadLine());
					byte g = Convert.ToByte(reader.ReadLine());
					byte b = Convert.ToByte(reader.ReadLine());
					byte a = Convert.ToByte(reader.ReadLine());

					delay_color = Color.FromArgb(a, r, g, b);
				}


				//Read type-specific data
				string sprites_path = temp_path + "\\shots\\sprite_info\\" + name + "_sprites.txt";

				using (StreamReader reader = new StreamReader(sprites_path))
				{
					if (type.Equals("Still"))
					{
						string sprite_name = reader.ReadLine();

						manager.AddStillShotType(name, sprite_name, collision, delay_color);
					}
					else if (type.Equals("Animation"))
					{
						manager.AddAnimationShotType(name, collision, delay_color);

						while (reader.Peek() != -1)
						{
							string sprite_name = reader.ReadLine();
							int delay = Convert.ToInt32(reader.ReadLine());

							manager.AddAnimationFrame(name, delay, sprite_name);
						}
					}
				}
			}

			//Read shot manager info
			using (StreamReader reader = new StreamReader(temp_path + "\\info.txt"))
			{
				string delay_sprite = reader.ReadLine();

				if (delay_sprite.Equals(""))
				{
					manager.DelaySprite = null;
				}
				else
				{
					manager.DelaySprite = manager.GetSpriteByName(delay_sprite);
				}

				manager.QuickExportLocation = reader.ReadLine();
			}

			//Clean up temp folder
			Directory.Delete(temp_path, true);

			return manager;
		}
	}
}