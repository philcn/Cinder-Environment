#include "cinder/app/App.h"
#include "cinder/app/RendererGl.h"
#include "cinder/gl/gl.h"
#include "cinder/CameraUi.h"
#include "cinder/ObjLoader.h"
#include "DOFFilter.h"

using namespace ci;
using namespace ci::app;
using namespace std;

#define ENABLE_SEM
#define ENABLE_DOF
//#define EXPORT_FRAMES
#define USE_ZBUFFER_DEPTH

class EnvironmentTestApp : public App {
public:
  void setup() override;
  void mouseMove(MouseEvent event) override;
  void mouseWheel(MouseEvent event) override;
  void update() override;
  void draw() override;
  void saveFrame();

private:
  CameraPersp mCam;
  gl::BatchRef mEnvironmentBatch;
  gl::BatchRef mObjectBatch;
  gl::TextureRef mEnvironmentTexture;
  gl::TextureRef mIlluminationTexture;
  DOFFilterRef mDOFFilter;
  vec2 mMousePos;
  float mDistanceTarget;
  float mDistance;
  float mFocalDepth;
  vec3 mCamTarget;
};

void EnvironmentTestApp::setup() {
  mCam.setPerspective(50.0f, getWindowAspectRatio(), 0.9f, 200.0f);

  mEnvironmentBatch = gl::Batch::create(geom::Sphere().subdivisions(128) >> geom::Scale(100), gl::getStockShader(gl::ShaderDef().texture()));
  
#ifdef ENABLE_SEM
  auto shader = gl::GlslProg::create(loadAsset("shaders/sem.vs"), loadAsset("shaders/sem.fs"));
  shader->uniform("tReflection", 0);
  shader->uniform("tMatCap", 1);
#else
  auto shader = gl::GlslProg::create(loadAsset("shaders/phong.vert"), loadAsset("shaders/phong.frag"));
#endif
  mObjectBatch = gl::Batch::create(ObjLoader(loadAsset("monkey.obj"), loadAsset("monkey.mtl")), shader);
  
  gl::Texture::Format textureFormat;
  textureFormat.enableMipmapping();
  textureFormat.setMinFilter(GL_LINEAR_MIPMAP_NEAREST);
  mEnvironmentTexture = gl::Texture::create(loadImage(loadAsset("360_night_street.jpg")), textureFormat);
  mIlluminationTexture = gl::Texture::create(loadImage(loadAsset("matcap.jpg")));
  
  mDOFFilter = DOFFilter::create();
  mDOFFilter->setFBOSamples(4);
  mDOFFilter->setManualFocalDepth(0.445);
  mDOFFilter->setHighlightGain(10);
  mDOFFilter->setHighlightThreshold(0.95);
  mDOFFilter->setAperture(20);
  mDOFFilter->setMaxBlur(3);
  mDOFFilter->setNumSamples(8);
  mDOFFilter->setNumRings(6);
  
  mDistanceTarget = 2;
#ifdef USE_ZBUFFER_DEPTH
  mFocalDepth = 0.445;
#else
  mFocalDepth = mDistanceTarget;
#endif
  
#ifdef EXPORT_FRAMES
  hideCursor();
#endif
}

void EnvironmentTestApp::mouseWheel(MouseEvent event) {
  mDistanceTarget += event.getWheelIncrement();
  mDistanceTarget = max(0.9f, min(4.0f, mDistanceTarget));
  
#ifdef USE_ZBUFFER_DEPTH
  mFocalDepth -= event.getWheelIncrement() * 0.05;
  mFocalDepth = max(0.3f, min(1.0f, mFocalDepth));
#else
  mFocalDepth -= event.getWheelIncrement() * 0.5;
  mFocalDepth = max(0.0f, min(200.0f, mFocalDepth));
#endif
}

void EnvironmentTestApp::mouseMove(MouseEvent event) {
  mMousePos = vec2(event.getX() - getWindowWidth() * 0.5, event.getY() - getWindowHeight() * 0.5);
}

void EnvironmentTestApp::update() {
  getWindow()->setTitle(to_string((int)getAverageFps()));
  
  mDistance += (mDistanceTarget - mDistance) * 0.036f;
  
  mCamTarget = vec3(sin(mMousePos.x * 0.005), 0, cos(mMousePos.x * 0.005));
  mCamTarget *= mDistance;
  mCamTarget.y = mMousePos.y * 0.005;
  
  vec3 cameraPos = mCam.getEyePoint();
  cameraPos += (mCamTarget - cameraPos) * 0.036f;
#ifdef ENABLE_DOF
  cameraPos = normalize(cameraPos) * 2.0f;
#else
  cameraPos = normalize(cameraPos) * mDistance;
#endif
  mCam.lookAt(cameraPos, vec3(0));
  
#ifdef ENABLE_DOF
  mDOFFilter->setManualFocalDepth(mFocalDepth);
#endif
}

void EnvironmentTestApp::draw() {
  gl::enableDepthRead();
  gl::enableDepthWrite();

#ifdef ENABLE_DOF
  mDOFFilter->beginRender();
#endif
  {
    gl::ScopedMatrices mtx;
    gl::setMatrices(mCam);
    gl::clear(Color::black());

    {
      gl::ScopedModelMatrix modelMtx;
      gl::scale(0.5, 0.5, 0.5);
#ifdef ENABLE_SEM
      gl::ScopedTextureBind tex0(mEnvironmentTexture, 0);
      gl::ScopedTextureBind tex1(mIlluminationTexture, 1);
#endif
      mObjectBatch->draw();
    }
    
    gl::ScopedFaceCulling culling(true, GL_FRONT);
    gl::ScopedTextureBind environmentTex(mEnvironmentTexture);
    mEnvironmentBatch->draw();
  }
#ifdef ENABLE_DOF
  mDOFFilter->endRender();
  mDOFFilter->drawFullscreen();
#endif

#ifdef EXPORT_FRAMES
  saveFrame();
#endif
}

void EnvironmentTestApp::saveFrame() {
  auto path = getAssetDirectories()[1];
  path.append("/export/frames/frame" + to_string(getElapsedFrames()) + ".png");
  writeImage(path, copyWindowSurface());
}

CINDER_APP(EnvironmentTestApp, RendererGl(RendererGl::Options().msaa(16)), [] (App::Settings *settings) {
//  settings->setWindowSize(1280, 800);
  settings->setWindowSize(640, 400);
  settings->setHighDensityDisplayEnabled();
})
