//
//  DOFFilter.h
//  DepthOfField
//
//  Created by Phil on 16/2/8.
//
//

#pragma once

#include "cinder/app/App.h"
#include "cinder/gl/gl.h"

using namespace ci;
using namespace ci::app;
using namespace std;

typedef std::shared_ptr<class DOFFilter> DOFFilterRef;

class DOFFilter {
public:
  DOFFilter();
  static DOFFilterRef create();

  void beginRender();
  void endRender();
  void drawFullscreen();

  bool getUseAutofocus();
  void setUseAutofocus(bool useAutofocus);
  float getManualFocalDepth();
  void setManualFocalDepth(float manualFocalDepth);
  vec2 getFocusInClipCoordinates();
  void setFocusInClipCoordinates(vec2 focusCenter);
  float getAperture();
  void setAperture(float aperture);
  float getMaxBlur();
  void setMaxBlur(float maxBlur);
  int getNumRings();
  void setNumRings(int numRings);
  int getNumSamples();
  void setNumSamples(int numSamples);
  float getHighlightThreshold();
  void setHighlightThreshold(float highlightThreshold);
  float getHighlightGain();
  void setHighlightGain(float highlightGain);
  float getBokehEdgeBias();
  void setBokehEdgeBias(float bokehEdgeBias);
  float getBokehFringe();
  void setBokehFringe(float bokehFringe);

  void setFBOSamples(int samples);
  void setHighDensityDisplayMode(bool isRetina);

private:
  gl::GlslProgRef mGlslProg;
  gl::FboRef mSceneFBO;
  gl::BatchRef mQuadBatch;

  bool mUseAutofocus;
  float mManualFocalDepth;
  vec2 mAutofocusCenter;
  float mAperture;
  float mMaxBlur;
  int mNumRings;
  int mNumSamples;
  float mHighlightThreshold;
  float mHighlightGain;
  float mBokehEdgeBias;
  float mBokehFringe;

  int mFBOSamples;
  bool mIsRetina;

  void createBuffers(int samples);
  int displayScale();
};
