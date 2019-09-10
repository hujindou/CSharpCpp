#include "stdafx.h"

#include "TestCppDll.h"

#include <cstdlib>
#include <iostream>
#include <ctime>

void TestCppDll::SetCsharpInstance::SetByteArrayWithRandom(cli::array<System::Byte>^ arr)
{
	unsigned char* buffer = new unsigned char[640 * 320 * 3];
	//unsigned char* buffer = (unsigned char *) malloc(640 * 320 * 3);

	std::srand(std::time(nullptr));

	for (int tmpi = 0; tmpi < 640 * 320 * 3; tmpi++)
	{
		buffer[tmpi] = (unsigned char)rand();
	}

	//System::IntPtr((void*)buffer) == (IntPtr)buffer
	System::Runtime::InteropServices::Marshal::Copy(
		(IntPtr)buffer,
		arr,
		0,
		640 * 320 * 3);

	delete[] buffer;
}

System::String ^ TestCppDll::SetCsharpInstance::FeedBackBuffer(System::Windows::Media::Imaging::WriteableBitmap^ wb)
{
	unsigned char* buffer = new unsigned char[640 * 320 * 3];
	//unsigned char* buffer = (unsigned char *) malloc(640 * 320 * 3);

	std::srand(std::time(nullptr));

	for (int tmpi = 0; tmpi < 640 * 320 * 3; tmpi++)
	{
		buffer[tmpi] = (unsigned char)rand();
	}

	memcpy(wb->BackBuffer.ToPointer(), buffer, 640 * 320 * 3);

	delete[] buffer;

	return "Succeed";
}

bool TestCppDll::SetCsharpInstance::FeedBackBuffer2(void* p)
{
	unsigned char* buffer = new unsigned char[640 * 320 * 3];
	//unsigned char* buffer = (unsigned char *) malloc(640 * 320 * 3);

	std::srand(std::time(nullptr));

	for (int tmpi = 0; tmpi < 640 * 320 * 3; tmpi++)
	{
		buffer[tmpi] = (unsigned char)rand();
	}

	memcpy(p, buffer, 640 * 320 * 3);

	delete[] buffer;

	return true;
}
