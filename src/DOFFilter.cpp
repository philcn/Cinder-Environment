//
//  DOFFilter.cpp
//  DepthOfField
//
//  Created by Phil on 16/2/8.
//
//

#include "DOFFilter.h"

#define ADVANCED_SHADER

DOFFilterRef DOFFilter::create() {
  return DOFFilterRef(new DOFFilter());
}

DOFFilter::DOFFilter() {
#ifdef ADVANCED_SHADER
  mGlslProg = gl::GlslProg::create(loadAsset("shaders/dofPass.vs"), loadAsset("shaders/dofPass.fs"));
#else
  mGlslProg = gl::GlslProg::create(loadAsset("shaders/basicBokeh.vs"), loadAsset("shaders/basicBokeh.fs"));
#endif
  mUseAutofocus = true;
  mManualFocalDepth = 0.95;
  mAutofocusCenter = vec2(0.5, 0.5);
  mAperture = 4.0;
  mMaxBlur = 1.5;
  mNumRings = 5;
  mNumSamples = 4;
  mHighlightThreshold = 0.5;
  mHighlightGain = 20.0;
  mBokehEdgeBias = 0.4;
  mBokehFringe = 0.5;
  mIsRetina = false;
  mFBOSamples = 1;

  createBuffers(mFBOSamples);
  
  mQuadBatch = gl::Batch::create(geom::Rect(), mGlslProg);
}

void DOFFilter::createBuffers(int samples) {
  mSceneFBO = gl::Fbo::create(toPixels(getWindowWidth()), toPixels(getWindowHeight()), gl::Fbo::Format().depthTexture().samples(samples));
}

void DOFFilter::beginRender() {
  mSceneFBO->bindFramebuffer();
}

void DOFFilter::endRender() {
  mSceneFBO->unbindFramebuffer();
}

void DOFFilter::drawFullscreen() {
  mGlslProg->uniform("uRenderedTexture", 0);
  mGlslProg->uniform("uDepthTexture", 1);
  mGlslProg->uniform("uFocalDepth", mManualFocalDepth);
  mGlslProg->uniform("uMaxBlur", mMaxBlur * displayScale());
#ifdef ADVANCED_SHADER
  mGlslProg->uniform("uRenderedTextureWidth", (float)mSceneFBO->getWidth());
  mGlslProg->uniform("uRenderedTextureHeight", (float)mSceneFBO->getHeight());
  mGlslProg->uniform("uUseAutofocus", mUseAutofocus);
  mGlslProg->uniform("uAutofocusCenter", mAutofocusCenter);
  mGlslProg->uniform("uNumRings", mNumRings);
  mGlslProg->uniform("uNumSamples", mNumSamples);
  mGlslProg->uniform("uHighlightThreshold", mHighlightThreshold);
  mGlslProg->uniform("uHighlightGain", mHighlightGain);
  mGlslProg->uniform("uBokehEdgeBias", mBokehEdgeBias);
  mGlslProg->uniform("uBokehFringe", mBokehFringe);
  mGlslProg->uniform("uAperture", mAperture / displayScale());
#else
  mGlslProg->uniform("uAspectRatio", getWindowAspectRatio());
  mGlslProg->uniform("uAperture", mAperture / displayScale() / 15);
#endif

  gl::ScopedTextureBind tex0(mSceneFBO->getColorTexture(), 0);
  gl::ScopedTextureBind tex1(mSceneFBO->getDepthTexture(), 1);
  
  const gl::ScopedViewport scopedViewport(ivec2(0), toPixels(getWindowSize()));
  const gl::ScopedMatrices scopedMatrices;
  gl::setMatricesWindow(toPixels(getWindowSize()));
  gl::translate(toPixels(getWindowSize() / 2));
  gl::scale(toPixels(getWindowSize()));
  
  gl::disableDepthRead();
  gl::disableDepthWrite();

  gl::clear(Color::black());
  mQuadBatch->draw();
}

bool DOFFilter::getUseAutofocus() {
  return mUseAutofocus;
}

void DOFFilter::setUseAutofocus(bool useAutofocus) {
  mUseAutofocus = useAutofocus;
}

float DOFFilter::getManualFocalDepth() {
  return mManualFocalDepth;
}

void DOFFilter::setManualFocalDepth(float manualFocalDepth) {
  mManualFocalDepth = manualFocalDepth;
  setUseAutofocus(false);
}

vec2 DOFFilter::getFocusInClipCoordinates() {
  return mAutofocusCenter;
}

void DOFFilter::setFocusInClipCoordinates(vec2 focusCenter) {
  mAutofocusCenter = focusCenter;
}
 
float DOFFilter::getAperture() {
  return mAperture;
}

void DOFFilter::setAperture(float aperture) {
  mAperture = aperture;
}

float DOFFilter::getMaxBlur() {
  return mMaxBlur;
}

void DOFFilter::setMaxBlur(float maxBlur) {
  mMaxBlur = maxBlur;
}

int DOFFilter::getNumRings() {
  return mNumRings;
}

void DOFFilter::setNumRings(int numRings) {
  mNumRings = numRings;
}

int DOFFilter::getNumSamples() {
  return mNumSamples;
}

void DOFFilter::setNumSamples(int numSamples) {
  mNumSamples = numSamples;
}

float DOFFilter::getHighlightThreshold() {
  return mHighlightThreshold;
}

void DOFFilter::setHighlightThreshold(float highlightThreshold) {
  mHighlightThreshold = highlightThreshold;
}

float DOFFilter::getHighlightGain() {
  return mHighlightGain;
}

void DOFFilter::setHighlightGain(float highlightGain) {
  mHighlightGain = highlightGain;
}

float DOFFilter::getBokehEdgeBias() {
  return mBokehEdgeBias;
}

void DOFFilter::setBokehEdgeBias(float bokehEdgeBias) {
  mBokehEdgeBias = bokehEdgeBias;
}

float DOFFilter::getBokehFringe() {
  return mBokehFringe;
}

void DOFFilter::setBokehFringe(float bokehFringe) {
  mBokehFringe = bokehFringe;
}

void DOFFilter::setFBOSamples(int samples) {
  mFBOSamples = samples;
  createBuffers(samples);
}

void DOFFilter::setHighDensityDisplayMode(bool isRetina) {
  mIsRetina = isRetina;
  createBuffers(mFBOSamples);
}

int DOFFilter::displayScale() {
  return mIsRetina ? 2 : 1;
}
