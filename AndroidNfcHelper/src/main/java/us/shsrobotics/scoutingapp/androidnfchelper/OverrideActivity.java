package us.shsrobotics.scoutingapp.androidnfchelper;

import android.content.Intent;
import android.os.Bundle;

import com.unity3d.player.UnityPlayerActivity;

public class OverrideActivity extends UnityPlayerActivity {
    private Intent _Intent;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        _Intent = getIntent();
        android.util.Log.d("UnityThing", "Recreating activity!");
    }

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        _Intent = intent;
        android.util.Log.d("UnityThing", "new intent!");
    }

    public boolean isNowNFCing() {
        boolean ret = _Intent != null && _Intent.getAction() == "android.nfc.action.NDEF_DISCOVERED";
        _Intent = null;
        return ret;
    }
}
