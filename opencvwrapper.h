#pragma once

#include <opencv2/objdetect.hpp>
#include <opencv2/core.hpp>
#include <opencv2/videoio.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>
#include <iostream>
#include <stdio.h>
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace cv;
using namespace std;

namespace opencvwrapper {
	public ref class OpenCvWrapper
	{
	private:
		VideoCapture* cap = nullptr;
		CascadeClassifier* cascade = nullptr;

		// TODO: Add your methods for this class here.
	public:

		bool OpenCamera();

		bool OpenCamera(int camId);

		bool CloseCamera();

		cli::array<int> ^ ListCameras();

		bool LoadXml(System::String^ xmllocation);

		System::String ^ FeedImg(cli::array<Byte> ^ bf);

		cli::array<int> ^ FeedImg2(cli::array<Byte> ^ bf);

		cli::array<int> ^ FeedImg3(cli::array<Byte> ^ bf,bool flipflag,int imgScale,int neighbor,float scaleFraction,int minSize,int maxSize);
	};
}
