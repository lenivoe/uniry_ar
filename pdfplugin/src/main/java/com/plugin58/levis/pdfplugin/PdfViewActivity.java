package com.plugin58.levis.pdfplugin;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.constraint.ConstraintLayout;
import android.support.v4.app.ActivityCompat;
import android.support.v7.app.AppCompatActivity;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

import com.github.barteksc.pdfviewer.PDFView;
import com.github.barteksc.pdfviewer.listener.OnPageChangeListener;
import com.github.barteksc.pdfviewer.listener.OnTapListener;
import com.github.barteksc.pdfviewer.util.FitPolicy;

import java.io.File;
import java.util.Timer;
import java.util.TimerTask;

public class PdfViewActivity extends AppCompatActivity implements View.OnClickListener {
    public static final String EXTRA_FILENAME = "FILENAME";
    private static final int PERMISSION_REQUEST_CODE = 0;
    private static final long UI_HIDE_DELAY = 5000;

    private int curPageNum;

    private PDFView pdfView = null;
    private PDFView.Configurator pdfViewConfigurator = null;
    private ConstraintLayout buttons_panel = null;
    private Button playButton = null;
    private Button stopButton = null;
    private Button timeButton = null;
    private Button crossButton = null;

    private Timer hideExtraUiTimer = null;
    private Timer scrollTimer = null;

    class ScrollDelayHolder {
        ScrollDelayHolder(int index) { ind = index; }

        long getDelay() { return delays[ind]; }
        String getDelayString() { return String.valueOf(getDelay() / 1000) + "c"; }
        void nextDelay() { ind = (ind + 1) % delays.length; }

        private final long[] delays = new long[] {
                1000, 2000, 3000, 4000, 5000, 7000, 10000, 13000, 15000
        };
        int ind;
    }

    private ScrollDelayHolder delays = new ScrollDelayHolder(4);

    @Override
    public void onClick(View v) {
        if(v == playButton) {
            playButton.setVisibility(View.INVISIBLE);
            stopButton.setVisibility(View.VISIBLE);
            if (scrollTimer != null)
                scrollTimer.cancel();
            scrollTimer = new Timer();
            scrollTimer.schedule(new TimerTask() {
                @Override
                public void run() {
                    if ((pdfView == null) || (pdfView.getCurrentPage() == pdfView.getPageCount() - 1)) {
                        this.cancel();
                        return;
                    }
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            pdfView.jumpTo(pdfView.getCurrentPage() + 1, true);
                        }
                    });
                }
            }, delays.getDelay(), delays.getDelay());
        } else if(v == stopButton) {
            playButton.setVisibility(View.VISIBLE);
            stopButton.setVisibility(View.INVISIBLE);
            if (scrollTimer != null) {
                scrollTimer.cancel();
                scrollTimer = null;
            }
        } else if(v == timeButton) {
            delays.nextDelay();
            timeButton.setText(delays.getDelayString());
        } else if(v == crossButton) {
            this.finish();
        }
    }

    protected void onSaveInstanceState(Bundle savingState) {
        super.onSaveInstanceState(savingState);
        savingState.putInt("curPageNum", curPageNum);
    }

    protected void onRestoreInstanceState(Bundle savedState) {
        super.onRestoreInstanceState(savedState);
        curPageNum = savedState.getInt("curPageNum");
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_pdf_view);

        pdfView = (PDFView) findViewById(R.id.pdfView);
        buttons_panel = (ConstraintLayout) findViewById(R.id.buttons_panel);
        playButton = (Button) findViewById(R.id.play_button);
        stopButton = (Button) findViewById(R.id.stop_button);
        timeButton = (Button) findViewById(R.id.time_button);
        crossButton = (Button) findViewById(R.id.cross_button);

        playButton.setOnClickListener(this);
        stopButton.setOnClickListener(this);
        timeButton.setOnClickListener(this);
        crossButton.setOnClickListener(this);


        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                != PackageManager.PERMISSION_GRANTED) {
            ActivityCompat.requestPermissions(this,
                    new String[] { Manifest.permission.READ_EXTERNAL_STORAGE },
                    PERMISSION_REQUEST_CODE);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                == PackageManager.PERMISSION_GRANTED) {
            showPdf();
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        if (requestCode == PERMISSION_REQUEST_CODE && grantResults.length == 1)
            if (grantResults[0] == PackageManager.PERMISSION_GRANTED)
                showPdf();
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
    }

    private void showPdf() {
        Intent intent = getIntent();
        String filename = intent.getStringExtra(EXTRA_FILENAME);
        //filename = "/storage/emulated/0/Download/Adventure Burner Transcript.pdf"; // FOR TEST

        if (filename != null) {
            File pdfFile = new File(filename);
            if (pdfFile.exists()) {
                if(pdfView == null)
                    pdfView = (PDFView) findViewById(R.id.pdfView);

                final PdfScrollHandle scrollHandle = new PdfScrollHandle(getApplicationContext(), false, false);
                scrollHandle.registerOnTouchCallback(new PdfScrollHandle.OnTouchCallback() {
                    @Override
                    public void onTouch(final PdfScrollHandle scrollHandle, MotionEvent event) {
                        if(hideExtraUiTimer != null) {
                            hideExtraUiTimer.cancel(); // отмена скрывания панельки и бегунка через UI_HIDE_DELAY миллисекунд
                            hideExtraUiTimer = null;
                        }
                        switch (event.getAction()) {
                            case MotionEvent.ACTION_CANCEL:
                            case MotionEvent.ACTION_UP:
                                if(hideExtraUiTimer == null)
                                    hideExtraUiTimer = new Timer();
                                hideExtraUiTimer.schedule(new TimerTask() {
                                    @Override
                                    public void run() {
                                        runOnUiThread(new Runnable() {
                                            @Override
                                            public void run() {
                                                buttons_panel.setVisibility(View.INVISIBLE);
                                                scrollHandle.manualHide();
                                            }
                                        });
                                    }
                                }, UI_HIDE_DELAY);
                                break;
                        }
                    }
                });
                pdfViewConfigurator = pdfView.fromFile(pdfFile);
                pdfViewConfigurator
                        .spacing(10) // in dp
                        .pageFitPolicy(FitPolicy.BOTH)
                        .onPageChange(new OnPageChangeListener() {
                            @Override
                            public void onPageChanged(int page, int pageCount) {
                                curPageNum = page;
                            }
                        })
                        .onTap(new OnTapListener() {
                            @Override
                            public boolean onTap(MotionEvent e) {
                                if(hideExtraUiTimer != null) {
                                    hideExtraUiTimer.cancel(); // отмена скрывания панельки и бегунка через UI_HIDE_DELAY миллисекунд
                                    hideExtraUiTimer = null;
                                }
                                if(e.getAction() == MotionEvent.ACTION_DOWN) {
                                    if(buttons_panel.getVisibility() == View.VISIBLE) {
                                        buttons_panel.setVisibility(View.INVISIBLE);
                                        scrollHandle.manualHide();
                                    } else {
                                        buttons_panel.setVisibility(View.VISIBLE);
                                        scrollHandle.manualShow();
                                        // скрыть панель и бегунок через UI_HIDE_DELAY миллисекунд
                                        if(hideExtraUiTimer == null)
                                            hideExtraUiTimer = new Timer();
                                        hideExtraUiTimer.schedule(new TimerTask() {
                                            @Override
                                            public void run() {
                                                runOnUiThread(new Runnable() {
                                                    @Override
                                                    public void run() {
                                                        buttons_panel.setVisibility(View.INVISIBLE);
                                                        scrollHandle.manualHide();
                                                    }
                                                });
                                            }
                                        }, UI_HIDE_DELAY);
                                    }
                                }
                                return false;
                            }
                        })
                        .scrollHandle(scrollHandle)
                        .defaultPage(curPageNum)
                        .load();
            }
        }
    }
}
