#include "cinder/app/App.h"
#include "cinder/app/RendererGl.h"
#include "cinder/gl/gl.h"
#include "cinder/CameraUi.h"

using namespace ci;
using namespace ci::app;
using namespace std;

class EnvironmentTestApp : public App {
public:
  void setup() override;
  void mouseDown(MouseEvent event) override;
  void update() override;
  void draw() override;

private:
  CameraPersp mCam;
  CameraUi mCamUi;
  gl::BatchRef mBatch;
};

void EnvironmentTestApp::setup() {
  mCam.setPerspective(60.0f, getWindowAspectRatio(), 1.0f, 1000.0f);
  mCam.lookAt(vec3(3, 3, 3), vec3(0, 0, 0));
  mCamUi = CameraUi(&mCam, getWindow(), -1);

  mBatch = gl::Batch::create(geom::Cube(), gl::getStockShader(gl::ShaderDef().color().lambert()));
}

void EnvironmentTestApp::mouseDown(MouseEvent event) {

}

void EnvironmentTestApp::update() {
  getWindow()->setTitle(to_string((int)getAverageFps()));
}

void EnvironmentTestApp::draw() {
  gl::enableDepthRead();
  gl::enableDepthWrite();

  gl::setMatrices(mCam);
  gl::clear(Color::black());

  gl::color(1, 0, 0);
  mBatch->draw();

  gl::disableDepthRead();
  gl::disableDepthWrite();
}

CINDER_APP(EnvironmentTestApp, RendererGl(RendererGl::Options().msaa(0)), [] (App::Settings *settings) {
  settings->setWindowSize(1280, 800);
  settings->setHighDensityDisplayEnabled();
})
