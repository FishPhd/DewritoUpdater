using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DoritoPatcherWPF
{
	public class SettingsViewModel : ViewModel
	{
		private PlayerSettingsViewModel _player;
		private VideoSettingsViewModel _video;
		private HostSettingsViewModel _host;

		public SettingsViewModel(DewritoSettings settings)
		{
			Load(settings);
		}

		public PlayerSettingsViewModel Player
		{
			get { return _player; }
			set { SetField(ref _player, value, () => Player); }
		}

		public VideoSettingsViewModel Video
		{
			get { return _video; }
			set { SetField(ref _video, value, () => Video); }
		}

		public HostSettingsViewModel Host
		{
			get { return _host; }
			set { SetField(ref _host, value, () => Host); }
		}

		private void Load(DewritoSettings settings)
		{
			_player = new PlayerSettingsViewModel(settings.Player ?? new DewritoPlayerSettings());
			_video = new VideoSettingsViewModel(settings.Video ?? new DewritoVideoSettings());
			_host = new HostSettingsViewModel(settings.Host ?? new DewritoHostSettings());
		}

		public void Save(DewritoSettings settings)
		{
			if (settings.Player == null)
				settings.Player = new DewritoPlayerSettings();
			if (settings.Video == null)
				settings.Video = new DewritoVideoSettings();
			if (settings.Host == null)
				settings.Host = new DewritoHostSettings();
			_player.Save(settings.Player);
			_video.Save(settings.Video);
			_host.Save(settings.Host);
		}
	}

	public class PlayerSettingsViewModel : ViewModel
	{
		private ArmorSettingsViewModel _armor;
		private ColorSettingsViewModel _colors;
		private string _name;

		public PlayerSettingsViewModel(DewritoPlayerSettings settings)
		{
			Load(settings);
		}

		public string Name
		{
			get { return _name; }
			set { SetField(ref _name, value, () => Name); }
		}

		public ArmorSettingsViewModel Armor
		{
			get { return _armor; }
			set { SetField(ref _armor, value, () => Armor); }
		}

		public ColorSettingsViewModel Colors
		{
			get { return _colors; }
			set { SetField(ref _colors, value, () => Colors); }
		}

		private void Load(DewritoPlayerSettings settings)
		{
			_name = settings.Name;
			_armor = new ArmorSettingsViewModel(settings.Armor ?? new DewritoArmorSettings());
			_colors = new ColorSettingsViewModel(settings.Colors ?? new DewritoColorSettings());
		}

		public void Save(DewritoPlayerSettings settings)
		{
			settings.Name = _name;

			if (settings.Armor == null)
				settings.Armor = new DewritoArmorSettings();
			if (settings.Colors == null)
				settings.Colors = new DewritoColorSettings();
			_armor.Save(settings.Armor);
			_colors.Save(settings.Colors);
		}
	}

	public class ArmorSettingsViewModel : ViewModel
	{
		private ArmorPieceViewModel _helmet, _chest, _shoulders, _arms, _legs;

		public ArmorSettingsViewModel(DewritoArmorSettings settings)
		{
			LoadArmorChoices();
			Load(settings);
		}

		public ArmorPieceViewModel Helmet
		{
			get { return _helmet; }
			set { SetField(ref _helmet, value, () => Helmet); }
		}

		public ArmorPieceViewModel Chest
		{
			get { return _chest; }
			set { SetField(ref _chest, value, () => Chest); }
		}

		public ArmorPieceViewModel Shoulders
		{
			get { return _shoulders; }
			set { SetField(ref _shoulders, value, () => Shoulders); }
		}

		public ArmorPieceViewModel Arms
		{
			get { return _arms; }
			set { SetField(ref _arms, value, () => Arms); }
		}

		public ArmorPieceViewModel Legs
		{
			get { return _legs; }
			set { SetField(ref _legs, value, () => Legs); }
		}

		public ObservableCollection<ArmorPieceViewModel> HelmetChoices { get; private set; }

		public ObservableCollection<ArmorPieceViewModel> ChestChoices { get; private set; }

		public ObservableCollection<ArmorPieceViewModel> ShoulderChoices { get; private set; }

		public ObservableCollection<ArmorPieceViewModel> ArmChoices { get; private set; }

		public ObservableCollection<ArmorPieceViewModel> LegChoices { get; private set; }

		private void LoadArmorChoices()
		{
			HelmetChoices = new ObservableCollection<ArmorPieceViewModel>(new List<ArmorPieceViewModel>
			{
				new ArmorPieceViewModel("base", "Base"),
				new ArmorPieceViewModel("stealth", "Stealth"),
				new ArmorPieceViewModel("air_assault", "Air Assault"),
				new ArmorPieceViewModel("renegade", "Renegade"),
				new ArmorPieceViewModel("nihard", "Nihard"),
				new ArmorPieceViewModel("gladiator", "Gladiator"),
				new ArmorPieceViewModel("mac", "Mac"),
				new ArmorPieceViewModel("shark", "Shark"),
				new ArmorPieceViewModel("juggernaut", "Juggernaut"),
				new ArmorPieceViewModel("dutch", "Dutch"),
				new ArmorPieceViewModel("chameleon", "Chameleon"),
				new ArmorPieceViewModel("halberd", "Halberd"),
				new ArmorPieceViewModel("cyclops", "Cyclops"),
				new ArmorPieceViewModel("scanner", "Scanner"),
				new ArmorPieceViewModel("mercenary", "Mercenary"),
				new ArmorPieceViewModel("hoplite", "Hoplite"),
				new ArmorPieceViewModel("ballista", "Ballista"),
				new ArmorPieceViewModel("strider", "Strider"),
				new ArmorPieceViewModel("demo", "Demo"),
				new ArmorPieceViewModel("orbital", "Orbital"),
				new ArmorPieceViewModel("spectrum", "Spectrum"),
				new ArmorPieceViewModel("gungnir", "Gungnir"),
				new ArmorPieceViewModel("hammerhead", "Hammerhead"),
				new ArmorPieceViewModel("omni", "Omni"),
				new ArmorPieceViewModel("oracle", "Oracle"),
				new ArmorPieceViewModel("silverback", "Silverback"),
				new ArmorPieceViewModel("widow_maker", "Widow Maker")
			}.OrderBy(a => a.DisplayName));
			ChestChoices = new ObservableCollection<ArmorPieceViewModel>(new List<ArmorPieceViewModel>
			{
				new ArmorPieceViewModel("base", "Base"),
				new ArmorPieceViewModel("stealth", "Stealth"),
				new ArmorPieceViewModel("air_assault", "Air Assault"),
				new ArmorPieceViewModel("renegade", "Renegade"),
				new ArmorPieceViewModel("nihard", "Nihard"),
				new ArmorPieceViewModel("gladiator", "Gladiator"),
				new ArmorPieceViewModel("mac", "Mac"),
				new ArmorPieceViewModel("shark", "Shark"),
				new ArmorPieceViewModel("juggernaut", "Juggernaut"),
				new ArmorPieceViewModel("dutch", "Dutch"),
				new ArmorPieceViewModel("chameleon", "Chameleon"),
				new ArmorPieceViewModel("halberd", "Halberd"),
				new ArmorPieceViewModel("cyclops", "Cyclops"),
				new ArmorPieceViewModel("scanner", "Scanner"),
				new ArmorPieceViewModel("mercenary", "Mercenary"),
				new ArmorPieceViewModel("hoplite", "Hoplite"),
				new ArmorPieceViewModel("ballista", "Ballista"),
				new ArmorPieceViewModel("strider", "Strider"),
				new ArmorPieceViewModel("demo", "Demo"),
				new ArmorPieceViewModel("orbital", "Orbital"),
				new ArmorPieceViewModel("spectrum", "Spectrum"),
				new ArmorPieceViewModel("gungnir", "Gungnir"),
				new ArmorPieceViewModel("hammerhead", "Hammerhead"),
				new ArmorPieceViewModel("omni", "Omni"),
				new ArmorPieceViewModel("oracle", "Oracle"),
				new ArmorPieceViewModel("silverback", "Silverback"),
				new ArmorPieceViewModel("widow_maker", "Widow Maker"),
				new ArmorPieceViewModel("tankmode_human", "Tankmode")
			}.OrderBy(a => a.DisplayName));
			ShoulderChoices = new ObservableCollection<ArmorPieceViewModel>(new List<ArmorPieceViewModel>
			{
				new ArmorPieceViewModel("base", "Base"),
				new ArmorPieceViewModel("stealth", "Stealth"),
				new ArmorPieceViewModel("air_assault", "Air Assault"),
				new ArmorPieceViewModel("renegade", "Renegade"),
				new ArmorPieceViewModel("nihard", "Nihard"),
				new ArmorPieceViewModel("gladiator", "Gladiator"),
				new ArmorPieceViewModel("mac", "Mac"),
				new ArmorPieceViewModel("shark", "Shark"),
				new ArmorPieceViewModel("juggernaut", "Juggernaut"),
				new ArmorPieceViewModel("dutch", "Dutch"),
				new ArmorPieceViewModel("chameleon", "Chameleon"),
				new ArmorPieceViewModel("halberd", "Halberd"),
				new ArmorPieceViewModel("cyclops", "Cyclops"),
				new ArmorPieceViewModel("scanner", "Scanner"),
				new ArmorPieceViewModel("mercenary", "Mercenary"),
				new ArmorPieceViewModel("hoplite", "Hoplite"),
				new ArmorPieceViewModel("ballista", "Ballista"),
				new ArmorPieceViewModel("strider", "Strider"),
				new ArmorPieceViewModel("demo", "Demo"),
				new ArmorPieceViewModel("orbital", "Orbital"),
				new ArmorPieceViewModel("spectrum", "Spectrum"),
				new ArmorPieceViewModel("gungnir", "Gungnir"),
				new ArmorPieceViewModel("hammerhead", "Hammerhead"),
				new ArmorPieceViewModel("omni", "Omni"),
				new ArmorPieceViewModel("oracle", "Oracle"),
				new ArmorPieceViewModel("silverback", "Silverback"),
				new ArmorPieceViewModel("widow_maker", "Widow Maker"),
				new ArmorPieceViewModel("tankmode_human", "Tankmode")
			}.OrderBy(a => a.DisplayName));
			ArmChoices = new ObservableCollection<ArmorPieceViewModel>(new List<ArmorPieceViewModel>
			{
				new ArmorPieceViewModel("base", "Base"),
				new ArmorPieceViewModel("stealth", "Stealth"),
				new ArmorPieceViewModel("air_assault", "Air Assault"),
				new ArmorPieceViewModel("renegade", "Renegade"),
				new ArmorPieceViewModel("nihard", "Nihard"),
				new ArmorPieceViewModel("gladiator", "Gladiator"),
				new ArmorPieceViewModel("mac", "Mac"),
				new ArmorPieceViewModel("shark", "Shark"),
				new ArmorPieceViewModel("juggernaut", "Juggernaut"),
				new ArmorPieceViewModel("dutch", "Dutch"),
				new ArmorPieceViewModel("chameleon", "Chameleon"),
				new ArmorPieceViewModel("halberd", "Halberd"),
				new ArmorPieceViewModel("cyclops", "Cyclops"),
				new ArmorPieceViewModel("scanner", "Scanner"),
				new ArmorPieceViewModel("mercenary", "Mercenary"),
				new ArmorPieceViewModel("hoplite", "Hoplite"),
				new ArmorPieceViewModel("ballista", "Ballista"),
				new ArmorPieceViewModel("strider", "Strider"),
				new ArmorPieceViewModel("demo", "Demo"),
				new ArmorPieceViewModel("orbital", "Orbital"),
				new ArmorPieceViewModel("spectrum", "Spectrum"),
				new ArmorPieceViewModel("gungnir", "Gungnir"),
				new ArmorPieceViewModel("hammerhead", "Hammerhead"),
				new ArmorPieceViewModel("omni", "Omni"),
				new ArmorPieceViewModel("oracle", "Oracle"),
				new ArmorPieceViewModel("silverback", "Silverback"),
				new ArmorPieceViewModel("widow_maker", "Widow Maker"),
				new ArmorPieceViewModel("tankmode_human", "Tankmode")
			}.OrderBy(a => a.DisplayName));
			LegChoices = new ObservableCollection<ArmorPieceViewModel>(new List<ArmorPieceViewModel>
			{
				new ArmorPieceViewModel("base", "Base"),
				new ArmorPieceViewModel("stealth", "Stealth"),
				new ArmorPieceViewModel("air_assault", "Air Assault"),
				new ArmorPieceViewModel("renegade", "Renegade"),
				new ArmorPieceViewModel("nihard", "Nihard"),
				new ArmorPieceViewModel("gladiator", "Gladiator"),
				new ArmorPieceViewModel("mac", "Mac"),
				new ArmorPieceViewModel("shark", "Shark"),
				new ArmorPieceViewModel("juggernaut", "Juggernaut"),
				new ArmorPieceViewModel("dutch", "Dutch"),
				new ArmorPieceViewModel("chameleon", "Chameleon"),
				new ArmorPieceViewModel("halberd", "Halberd"),
				new ArmorPieceViewModel("cyclops", "Cyclops"),
				new ArmorPieceViewModel("scanner", "Scanner"),
				new ArmorPieceViewModel("mercenary", "Mercenary"),
				new ArmorPieceViewModel("hoplite", "Hoplite"),
				new ArmorPieceViewModel("ballista", "Ballista"),
				new ArmorPieceViewModel("strider", "Strider"),
				new ArmorPieceViewModel("demo", "Demo"),
				new ArmorPieceViewModel("orbital", "Orbital"),
				new ArmorPieceViewModel("spectrum", "Spectrum"),
				new ArmorPieceViewModel("gungnir", "Gungnir"),
				new ArmorPieceViewModel("hammerhead", "Hammerhead"),
				new ArmorPieceViewModel("omni", "Omni"),
				new ArmorPieceViewModel("oracle", "Oracle"),
				new ArmorPieceViewModel("silverback", "Silverback"),
				new ArmorPieceViewModel("widow_maker", "Widow Maker"),
				new ArmorPieceViewModel("tankmode_human", "Tankmode")
			}.OrderBy(a => a.DisplayName));
		}

		private void Load(DewritoArmorSettings settings)
		{
			_helmet = FindArmor(HelmetChoices, settings.Helmet);
			_chest = FindArmor(ChestChoices, settings.Chest);
			_shoulders = FindArmor(ShoulderChoices, settings.Shoulders);
			_arms = FindArmor(ArmChoices, settings.Arms);
			_legs = FindArmor(LegChoices, settings.Legs);
		}

		private static ArmorPieceViewModel FindArmor(ObservableCollection<ArmorPieceViewModel> armor, string internalName)
		{
			// First try to find the armor, and if it's not found, then fall back on base, and if that's not found, then use the first piece
			return armor.FirstOrDefault(a => a.InternalName == internalName) ??
				   (armor.FirstOrDefault(a => a.InternalName == "base") ??
				   (armor.FirstOrDefault()));
		}

		public void Save(DewritoArmorSettings settings)
		{
			settings.Helmet = _helmet.InternalName;
			settings.Chest = _chest.InternalName;
			settings.Shoulders = _shoulders.InternalName;
			settings.Arms = _arms.InternalName;
			settings.Legs = _legs.InternalName;
		}
	}

	public class ColorSettingsViewModel : ViewModel
	{
		private Color _primary, _secondary, _visor, _lights, _holo;

		public ColorSettingsViewModel(DewritoColorSettings settings)
		{
			Load(settings);
		}

		public Color Primary
		{
			get { return _primary; }
			set { SetField(ref _primary, value, () => Primary); }
		}

		public Color Secondary
		{
			get { return _secondary; }
			set { SetField(ref _secondary, value, () => Secondary); }
		}

		public Color Visor
		{
			get { return _visor; }
			set { SetField(ref _visor, value, () => Visor); }
		}

		public Color Lights
		{
			get { return _lights; }
			set { SetField(ref _lights, value, () => Lights); }
		}

		public Color Holo
		{
			get { return _holo; }
			set { SetField(ref _holo, value, () => Holo); }
		}

		private void Load(DewritoColorSettings settings)
		{
			_primary = settings.Primary.AsColor();
			_secondary = settings.Secondary.AsColor();
			_visor = settings.Visor.AsColor();
			_lights = settings.Lights.AsColor();
			_holo = settings.Holo.AsColor();
		}

		public void Save(DewritoColorSettings settings)
		{
			settings.Primary = new RgbColorSetting(_primary);
			settings.Secondary = new RgbColorSetting(_secondary);
			settings.Visor = new RgbColorSetting(_visor);
			settings.Lights = new RgbColorSetting(_lights);
			settings.Holo = new RgbColorSetting(_holo);
		}
	}

	public class ArmorPieceViewModel
	{
		public ArmorPieceViewModel(string internalName, string displayName)
		{
			InternalName = internalName;
			DisplayName = displayName;
		}

		public string InternalName { get; private set; }

		public string DisplayName { get; private set; }
	}

	public class VideoSettingsViewModel : ViewModel
	{
		private float _fov;

		public VideoSettingsViewModel(DewritoVideoSettings settings)
		{
			Load(settings);
		}

		public float FieldOfView
		{
			get { return _fov; }
			set { SetField(ref _fov, value, () => FieldOfView); }
		}

		private void Load(DewritoVideoSettings settings)
		{
			_fov = settings.Fov;
		}

		public void Save(DewritoVideoSettings settings)
		{
			settings.Fov = _fov;
		}
	}

	public class HostSettingsViewModel : ViewModel
	{
		private int _countdown;

		public HostSettingsViewModel(DewritoHostSettings settings)
		{
			Load(settings);
		}

		public int Countdown
		{
			get { return _countdown; }
			set { SetField(ref _countdown, value, () => Countdown); }
		}

		private void Load(DewritoHostSettings settings)
		{
			_countdown = settings.Countdown;
		}

		public void Save(DewritoHostSettings settings)
		{
			settings.Countdown = _countdown;
		}
	}
}
