#include "stdafx.h"

#include "opencvwrapper.h"

bool opencvwrapper::OpenCvWrapper::OpenCamera()
{
	int deviceID = 0;             // 0 = open default camera
	int apiID = cv::CAP_ANY;      // 0 = autodetect default API

	if (cap == nullptr)
	{
		cap = new VideoCapture();
	}
	else
	{
		return cap->isOpened();
	}

	// open selected camera using selected API
	cap->open(deviceID + apiID);

	if (cap->isOpened()) {
		cap->set(CAP_PROP_FRAME_WIDTH, 1280);
		cap->set(CAP_PROP_FRAME_HEIGHT, 720);
	}

	return cap->isOpened();
}

bool opencvwrapper::OpenCvWrapper::OpenCamera(int camId)
{
	if (cap == nullptr)
	{
		cap = new VideoCapture();
	}
	else
	{
		return cap->isOpened();
	}

	// open selected camera using selected API
	cap->open(camId);

	if (cap->isOpened()) {
		cap->set(CAP_PROP_FRAME_WIDTH, 1280);
		cap->set(CAP_PROP_FRAME_HEIGHT, 720);
	}

	return cap->isOpened();
}

bool opencvwrapper::OpenCvWrapper::CloseCamera()
{
	if (cap == nullptr)
	{
	}
	else
	{
		cap->release();
		cap = nullptr;
	}

	return true;
}

bool opencvwrapper::OpenCvWrapper::LoadXml(System::String^ xmllocation)
{
	if (cascade == nullptr)
	{
		cascade = new CascadeClassifier();
	}

	msclr::interop::marshal_context context;
	std::string standardString = context.marshal_as<std::string>(xmllocation);

	if (!cascade->load(standardString))
	{
		return false;
	}

	return true;
}

System::String^ opencvwrapper::OpenCvWrapper::FeedImg(cli::array<Byte> ^ wb)
{
	Mat raw;

	Mat frame;

	cap->read(raw);

	if (raw.empty())
		return nullptr;

	flip(raw, frame, 1);

	int totSize = frame.total() * frame.elemSize();

	System::Runtime::InteropServices::Marshal::Copy((IntPtr)(frame.data),
		wb, 0, 1280 * 720 * 3);

	double t = 0;

	double scale = 4;

	vector<Rect> faces;

	const static Scalar colors[] =
	{
		Scalar(255,0,0),
		Scalar(255,128,0),
		Scalar(255,255,0),
		Scalar(0,255,0),
		Scalar(0,128,255),
		Scalar(0,255,255),
		Scalar(0,0,255),
		Scalar(255,0,255)
	};
	Mat gray, smallImg;

	cvtColor(frame, gray, COLOR_BGR2GRAY);
	double fx = 1 / scale;
	resize(gray, smallImg, Size(), fx, fx, INTER_LINEAR_EXACT);
	equalizeHist(smallImg, smallImg);

	t = (double)getTickCount();
	cascade -> detectMultiScale(smallImg, faces,
		1.1, 2, 0
		//|CASCADE_FIND_BIGGEST_OBJECT
		//|CASCADE_DO_ROUGH_SEARCH
		| CASCADE_SCALE_IMAGE,
		Size(30, 30));

	t = (double)getTickCount() - t;

	printf("detection time = %g ms\n", t * 1000 / getTickFrequency());
	
	for (size_t i = 0; i < faces.size(); i++)
	{
		Rect r = faces[i];
		Mat smallImgROI;
		Point center;
		Scalar color = colors[i % 8];
		int radius;

		double aspect_ratio = (double)r.width / r.height;

		if (0.75 < aspect_ratio && aspect_ratio < 1.3)
		{
			center.x = cvRound((r.x + r.width*0.5)*scale);
			center.y = cvRound((r.y + r.height*0.5)*scale);
			radius = cvRound((r.width + r.height)*0.25*scale);
			circle(frame, center, radius, color, 3, 8, 0);
		}
		else
			rectangle(frame, Point(cvRound(r.x*scale), cvRound(r.y*scale)),
				Point(cvRound((r.x + r.width - 1)*scale), cvRound((r.y + r.height - 1)*scale)),
				color, 3, 8, 0);
	}

	return "detection time = [" + t * 1000 / getTickFrequency() + "] faces.size = [" + faces.size() + "]";

	//return "mat.cols=[ " + frame.cols + "] mat.rows=[" + frame.rows + "] mat.total=[" + frame.total() + "] mat.elemSize=[" + frame.elemSize() + "] mat.Size=[" + totSize + "] ";
}

cli::array<int>^ opencvwrapper::OpenCvWrapper::FeedImg2(cli::array<Byte> ^ wb)
{
	Mat raw;

	Mat frame;

	cap->read(raw);

	if (raw.empty())
		return nullptr;

	flip(raw, frame, 1);

	int totSize = frame.total() * frame.elemSize();

	System::Runtime::InteropServices::Marshal::Copy((IntPtr)(frame.data),
		wb, 0, 1280 * 720 * 3);

	double t = 0;

	double scale = 4;

	vector<Rect> faces;

	const static Scalar colors[] =
	{
		Scalar(255,0,0),
		Scalar(255,128,0),
		Scalar(255,255,0),
		Scalar(0,255,0),
		Scalar(0,128,255),
		Scalar(0,255,255),
		Scalar(0,0,255),
		Scalar(255,0,255)
	};
	Mat gray, smallImg;

	cvtColor(frame, gray, COLOR_BGR2GRAY);
	double fx = 1 / scale;
	resize(gray, smallImg, Size(), fx, fx, INTER_LINEAR_EXACT);
	equalizeHist(smallImg, smallImg);

	t = (double)getTickCount();
	cascade->detectMultiScale(smallImg, faces,
		1.1, 2, 0
		//|CASCADE_FIND_BIGGEST_OBJECT
		//|CASCADE_DO_ROUGH_SEARCH
		| CASCADE_SCALE_IMAGE,
		Size(30, 30));

	t = (double)getTickCount() - t;

	//printf("detection time = %g ms\n", t * 1000 / getTickFrequency());

	auto ar = gcnew cli::array<int>(faces.size() * 4);

	for (size_t i = 0; i < faces.size(); i++)
	{
		Rect r = faces[i];
		Mat smallImgROI;
		Point center;
		Scalar color = colors[i % 8];
		int radius;

		double aspect_ratio = (double)r.width / r.height;

		//if (0.75 < aspect_ratio && aspect_ratio < 1.3)
		//{
		//	center.x = cvRound((r.x + r.width*0.5)*scale);
		//	center.y = cvRound((r.y + r.height*0.5)*scale);
		//	radius = cvRound((r.width + r.height)*0.25*scale);
		//	circle(frame, center, radius, color, 3, 8, 0);
		//}
		//else
		//	rectangle(frame, Point(cvRound(r.x*scale), cvRound(r.y*scale)),
		//		Point(cvRound((r.x + r.width - 1)*scale), cvRound((r.y + r.height - 1)*scale)),
		//		color, 3, 8, 0);

		ar[i * 4] = cvRound(r.x*scale);
		ar[i * 4 + 1] = cvRound(r.y*scale);
		ar[i * 4 + 2] = cvRound(r.width*scale);
		ar[i * 4 + 3] = cvRound(r.height*scale);
	}

	return ar;
	//return "mat.cols=[ " + frame.cols + "] mat.rows=[" + frame.rows + "] mat.total=[" + frame.total() + "] mat.elemSize=[" + frame.elemSize() + "] mat.Size=[" + totSize + "] ";
}

cli::array<int> ^ opencvwrapper::OpenCvWrapper::FeedImg3(cli::array<Byte> ^ bf, bool flipflag, int imgScale, int neighbor, float scaleFraction, int minSize, int maxSize)
{
	Mat raw;

	Mat frame;

	cap->read(raw);

	if (raw.empty())
		return nullptr;

	flip(raw, frame, 1);

	int totSize = frame.total() * frame.elemSize();

	System::Runtime::InteropServices::Marshal::Copy((IntPtr)(frame.data),
		bf, 0, 1280 * 720 * 3);

	double t = 0;

	double scale = imgScale;

	vector<Rect> faces, faces2;

	Mat gray, smallImg;

	cvtColor(frame, gray, COLOR_BGR2GRAY);
	double fx = 1 / scale;

	resize(gray, smallImg, Size(), fx, fx, INTER_LINEAR_EXACT);
	equalizeHist(smallImg, smallImg);

	t = (double)getTickCount();
	cascade->detectMultiScale(smallImg, faces,
		scaleFraction, neighbor, 0
		//|CASCADE_FIND_BIGGEST_OBJECT
		//|CASCADE_DO_ROUGH_SEARCH
		| CASCADE_SCALE_IMAGE,
		Size(minSize, minSize), Size(maxSize, maxSize));

	if (flipflag)
	{
		flip(smallImg, smallImg, 1);

		cascade->detectMultiScale(smallImg, faces2,
			scaleFraction, neighbor, 0
			//|CASCADE_FIND_BIGGEST_OBJECT
			//|CASCADE_DO_ROUGH_SEARCH
			| CASCADE_SCALE_IMAGE,
			Size(minSize, minSize), Size(maxSize, maxSize));
		for (vector<Rect>::const_iterator r = faces2.begin(); r != faces2.end(); ++r)
		{
			faces.push_back(Rect(smallImg.cols - r->x - r->width, r->y, r->width, r->height));
		}
	}

	t = (double)getTickCount() - t;

	auto ar = gcnew cli::array<int>(faces.size() * 4);

	for (size_t i = 0; i < faces.size(); i++)
	{
		Rect r = faces[i];

		double aspect_ratio = (double)r.width / r.height;

		ar[i * 4] = cvRound(r.x*scale);
		ar[i * 4 + 1] = cvRound(r.y*scale);
		ar[i * 4 + 2] = cvRound(r.width*scale);
		ar[i * 4 + 3] = cvRound(r.height*scale);
	}

	return ar;
}


cli::array<int>^ opencvwrapper::OpenCvWrapper::ListCameras()
{
	auto ar = gcnew cli::array<int>(10);

	for (int device = 0; device < 10; device++)
	{
		VideoCapture temp(device);

		if (!temp.isOpened())
		{
			temp.release();
			ar[device] = device;
		}
		else
		{
			ar[device] = -1;
		}
	}

	return ar;
}
