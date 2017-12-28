using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ZXing.QrCode.Internal;

public class OptionsHelper : MonoBehaviour
{
	public Image NFCCheckmark;
	public Text NFCErrorText;
	public Dropdown QRVersionSelector;
	public Dropdown QRErrorCorrectionLevelSelector;

	// Use this for initialization
	void Start()
	{
		NFCCheckmark.enabled = Options.Inst.IsNFCEnabled;
		if (!Options.Inst.DeviceHasNFC)
			NFCErrorText.text = "Sorry, your device doesn't support NFC.";
		else if (!Options.Inst.NFCSystemOn)
			NFCErrorText.text = "NFC is not enabled. Try enabling it in settings.";
		else
			NFCErrorText.enabled = false;

		QRVersionSelector.value = 40 - Options.Inst.QRVersion;
		QRErrorCorrectionLevelSelector.value = Options.Inst.QRErrorCorrection == ErrorCorrectionLevel.L ? 0 :
			Options.Inst.QRErrorCorrection == ErrorCorrectionLevel.M ? 1 :
			Options.Inst.QRErrorCorrection == ErrorCorrectionLevel.Q ? 2 : 3;
	}

	public void ClickNFCToggle()
	{
		if (Options.Inst.NFCSystemOn)
		{
			Options.Inst.IsNFCEnabled = !Options.Inst.IsNFCEnabled;
			NFCCheckmark.enabled = Options.Inst.IsNFCEnabled;
		}
	}

	public void OnVersionValueChange(int newValue)
	{
		Options.Inst.QRVersion = (byte)(40 - newValue);
	}

	public void OnErrorCorrectionValueChange(int newValue)
	{
		Options.Inst.QRErrorCorrection = newValue == 0 ? ErrorCorrectionLevel.L :
			newValue == 1 ? ErrorCorrectionLevel.M :
			newValue == 2 ? ErrorCorrectionLevel.Q : ErrorCorrectionLevel.H;
	}
}

public class Options
{
	/// <summary>
	/// The main options instance.
	/// </summary>
	public static Options Inst { get; } = new Options();

	/// <summary>
	/// Whether the device supports Near Field Communication.
	/// </summary>
	public bool DeviceHasNFC { get; private set; }

	/// <summary>
	/// Whether the device has Near Field Communication enabled in the system settings.
	/// </summary>
	public bool NFCSystemOn { get; private set; }

	private bool _NFCEnabled = true;
	/// <summary>
	/// Whether NFC is set to the prefered import and export method.
	/// </summary>
	public bool IsNFCEnabled
	{
		get
		{
			return _NFCEnabled;
		}
		set
		{
			_NFCEnabled = value;
			SaveOptions();
		}
	}

	private byte _QRVersion = 32;
	/// <summary>
	/// The QR version for QR code export mode.
	/// </summary>
	public byte QRVersion
	{
		get
		{
			return _QRVersion;
		}
		set
		{
			_QRVersion = value;
			SaveOptions();
		}
	}

	private ErrorCorrectionLevel _QRErrorCorrection = ErrorCorrectionLevel.M;
	/// <summary>
	/// The error correction level for QR code export mode.
	/// </summary>
	public ErrorCorrectionLevel QRErrorCorrection
	{
		get
		{
			return _QRErrorCorrection;
		}
		set
		{
			_QRErrorCorrection = value;
			SaveOptions();
		}
	}

	private string _OptionsLoc = Path.Combine(Application.persistentDataPath, "options.dat");

	public Options()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
			AndroidJavaObject activity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

			AndroidJavaObject nfcManager = activity.Call<AndroidJavaObject>("getSystemService",
				contextClass.GetStatic<string>("NFC_SERVICE"));

			AndroidJavaObject adapter = nfcManager.Call<AndroidJavaObject>("getDefaultAdapter");
			if (adapter == null)
			{
				DeviceHasNFC = false;
			}
			else
			{
				DeviceHasNFC = true;
				NFCSystemOn = adapter.Call<bool>("isEnabled");
			}
		}
		LoadOptions();
	}

	public void LoadOptions()
	{
		if (File.Exists(_OptionsLoc))
		{
			try
			{
				using (FileStream fs = File.OpenRead(_OptionsLoc))
				{
					byte b0 = (byte)fs.ReadByte();
					_NFCEnabled = ((b0 >> 7) & 0x1) == 1;
					_QRVersion = (byte)(b0 & 0x7F);
					byte errorLvl = (byte)fs.ReadByte();
					if (errorLvl == 0)
						_QRErrorCorrection = ErrorCorrectionLevel.L;
					else if (errorLvl == 1)
						_QRErrorCorrection = ErrorCorrectionLevel.M;
					else if (errorLvl == 2)
						_QRErrorCorrection = ErrorCorrectionLevel.Q;
					else
						_QRErrorCorrection = ErrorCorrectionLevel.H;
				}
			}
			catch
			{
				SaveOptions();
			}
		}
		else
		{
			SaveOptions();
		}
		if (!NFCSystemOn && _NFCEnabled)
		{
			_NFCEnabled = false;
			SaveOptions();
		}
	}

	public void SaveOptions()
	{
		using (FileStream fs = File.Open(_OptionsLoc, FileMode.Create))
		{
			fs.WriteByte((byte)(_QRVersion | (_NFCEnabled ? 0x80 : 0)));
			if (_QRErrorCorrection == ErrorCorrectionLevel.L)
				fs.WriteByte(0);
			else if (_QRErrorCorrection == ErrorCorrectionLevel.M)
				fs.WriteByte(1);
			else if (_QRErrorCorrection == ErrorCorrectionLevel.Q)
				fs.WriteByte(2);
			else if (_QRErrorCorrection == ErrorCorrectionLevel.H)
				fs.WriteByte(3);
		}
	}
}
