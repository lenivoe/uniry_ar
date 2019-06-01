package com.plugin58.levis.pdfplugin;

import android.annotation.SuppressLint;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.support.v4.content.ContextCompat;
import android.util.TypedValue;
import android.view.MotionEvent;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;

import com.github.barteksc.pdfviewer.PDFView;
import com.github.barteksc.pdfviewer.scroll.ScrollHandle;
import com.github.barteksc.pdfviewer.util.Util;

public class PdfScrollHandle extends RelativeLayout implements ScrollHandle {
    interface OnTouchCallback {
        void onTouch(PdfScrollHandle scrollHandle, MotionEvent event);
    }
    OnTouchCallback onTouchCallback;

    public void registerOnTouchCallback(OnTouchCallback callback) {
        onTouchCallback = callback;
    }

    private final static int HANDLE_LONG = 65;
    private final static int HANDLE_SHORT = 40;
    private final static int DEFAULT_TEXT_SIZE = 16;

    protected boolean isAutoVisibility = true; // for manual control of visibility

    protected TextView textView;
    protected Context context;
    private boolean inverted;
    private PDFView pdfView;

    private Handler handler = new Handler();
    private Runnable hidePageScrollerRunnable = new Runnable() {
        @Override
        public void run() {
            hide();
        }
    };

    public PdfScrollHandle(Context context) {
        this(context, false, true);
    }

    public PdfScrollHandle(Context context, boolean inverted, boolean autoVisibility) {
        super(context);
        this.context = context;
        this.inverted = inverted;
        textView = new TextView(context);
        setVisibility(INVISIBLE);
        setTextColor(Color.BLACK);
        setTextSize(DEFAULT_TEXT_SIZE);
        isAutoVisibility = autoVisibility;
    }

    @SuppressLint("ObsoleteSdkInt")
    @Override
    public void setupLayout(PDFView pdfView) {
        int align, width, height;
        Drawable background;
        // determine handler position, default is right (when scrolling vertically) or bottom (when scrolling horizontally)
        if (pdfView.isSwipeVertical()) {
            width = HANDLE_LONG;
            height = HANDLE_SHORT;
            if (inverted) { // left
                align = ALIGN_PARENT_LEFT;
                background = ContextCompat.getDrawable(context, R.drawable.default_scroll_handle_left);
            } else { // right
                align = ALIGN_PARENT_RIGHT;
                background = ContextCompat.getDrawable(context, R.drawable.default_scroll_handle_right);
            }
        } else {
            width = HANDLE_SHORT;
            height = HANDLE_LONG;
            if (inverted) { // top
                align = ALIGN_PARENT_TOP;
                background = ContextCompat.getDrawable(context, R.drawable.default_scroll_handle_top);
            } else { // bottom
                align = ALIGN_PARENT_BOTTOM;
                background = ContextCompat.getDrawable(context, R.drawable.default_scroll_handle_bottom);
            }
        }

        if (android.os.Build.VERSION.SDK_INT < android.os.Build.VERSION_CODES.JELLY_BEAN) {
            setBackgroundDrawable(background);
        } else {
            setBackground(background);
        }

        LayoutParams lp = new LayoutParams(Util.getDP(context, width), Util.getDP(context, height));
        lp.setMargins(0, 0, 0, 0);

        LayoutParams tvlp = new LayoutParams(ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        tvlp.addRule(RelativeLayout.CENTER_IN_PARENT, RelativeLayout.TRUE);

        addView(textView, tvlp);

        lp.addRule(align);
        pdfView.addView(this, lp);

        this.pdfView = pdfView;
    }

    @Override
    public void destroyLayout() {
        pdfView.removeView(this);
    }

    @Override
    public void setScroll(float position) {
        if (!shown()) {
            show();
        } else {
            handler.removeCallbacks(hidePageScrollerRunnable);
        }

        float size = calcSize();
        if (pdfView.isSwipeVertical()) {
            setY(pdfView.getPaddingTop() + position * size);
        } else {
            setX(pdfView.getPaddingLeft() + position * size);
        }

        invalidate();
    }

    private float calcSize() {
        if (pdfView.isSwipeVertical()) {
            return pdfView.getHeight() - pdfView.getPaddingTop() - pdfView.getPaddingBottom() - getHeight();
        } else {
            return pdfView.getWidth() - pdfView.getPaddingLeft() - pdfView.getPaddingRight() - getWidth();
        }
    }

    @Override
    public void hideDelayed() {
        if(isAutoVisibility) handler.postDelayed(hidePageScrollerRunnable, 1000);
    }

    @Override
    public void setPageNum(int pageNum) {
        String text = String.valueOf(pageNum);
        if (!textView.getText().equals(text))
            textView.setText(text);
    }

    @Override
    public boolean shown() {
        return getVisibility() == VISIBLE;
    }

    @Override
    public void show() {
        if(isAutoVisibility) setVisibility(VISIBLE);
    }

    @Override
    public void hide() {
        if(isAutoVisibility) setVisibility(INVISIBLE);
    }

    public void manualShow() {
        setVisibility(VISIBLE);
    }

    public void manualHide() {
        setVisibility(INVISIBLE);
    }

    public void setTextColor(int color) {
        textView.setTextColor(color);
    }

    /**
     * @param size text size in dp
     */
    public void setTextSize(int size) {
        textView.setTextSize(TypedValue.COMPLEX_UNIT_DIP, size);
    }

    private boolean isPDFViewReady() {
        return pdfView != null && pdfView.getPageCount() > 0 && !pdfView.documentFitsView();
    }

    private float calcRelativePos(MotionEvent event) {
        float size = calcSize();
        float curPos;
        if (pdfView.isSwipeVertical()) {
            curPos = event.getRawY() - pdfView.getY() - pdfView.getPaddingTop() - getHeight() / 2;
        } else {
            curPos = event.getRawX() - pdfView.getX() - pdfView.getPaddingLeft() - getWidth() / 2;
        }
        return Math.max(0, Math.min(curPos, size)) / size;
    }

    @SuppressLint("ClickableViewAccessibility")
    @Override
    public boolean onTouchEvent(MotionEvent event) {
        if (!isPDFViewReady())
            return super.onTouchEvent(event);

        onTouchCallback.onTouch(this, event);

        switch (event.getAction()) {
            case MotionEvent.ACTION_DOWN:
                pdfView.stopFling();
                handler.removeCallbacks(hidePageScrollerRunnable);
            case MotionEvent.ACTION_MOVE:
                float pos = calcRelativePos(event);
                setScroll(pos);
                pdfView.setPositionOffset(pos, false);
                return true;

            case MotionEvent.ACTION_CANCEL:
            case MotionEvent.ACTION_UP:
                hideDelayed();
                return true;
        }

        return super.onTouchEvent(event);
    }
}
