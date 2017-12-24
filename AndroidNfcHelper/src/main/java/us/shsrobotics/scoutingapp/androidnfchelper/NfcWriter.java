package us.shsrobotics.scoutingapp.androidnfchelper;

import android.app.Activity;
import android.nfc.NdefMessage;
import android.nfc.NdefRecord;
import android.nfc.NfcAdapter;
import android.nfc.NfcEvent;

public class NfcWriter implements NfcAdapter.OnNdefPushCompleteCallback, NfcAdapter.CreateNdefMessageCallback {
    private byte[] _Msg = null;

    @Override
    public NdefMessage createNdefMessage(NfcEvent event) {
        if (_Msg == null)
            return null;

        NdefRecord[] recordsToAttach = createNdefRecord();
        return new NdefMessage(recordsToAttach);
    }

    private NdefRecord[] createNdefRecord() {
        NdefRecord[] records = new NdefRecord[1];
		android.util.Log.d("Hello");
        records[0] = NdefRecord.createMime("text/plain", new byte[] { (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o' } );
        return records;
    }

    @Override
    public void onNdefPushComplete(NfcEvent event) {
        _Msg = null;
    }

    public boolean broadcasting() {
        return _Msg != null;
    }

    public void setMessage(byte[] s) {
        _Msg = s;
    }

    public void register(Activity activity) {
        NfcAdapter adapter = NfcAdapter.getDefaultAdapter(activity);

        // Called to get what to send
        adapter.setNdefPushMessageCallback(this, activity);
        // Called if message is sent successfully
        adapter.setOnNdefPushCompleteCallback(this, activity);
    }
}
