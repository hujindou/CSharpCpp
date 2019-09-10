#pragma once

using namespace System;

namespace TestCppDll {
	public ref class SetCsharpInstance
	{
		// TODO: Add your methods for this class here.

	public:
		void SetByteArrayWithRandom(cli::array<System::Byte>^ arr);

		System::String ^ FeedBackBuffer(System::Windows::Media::Imaging::WriteableBitmap^ wb);

		bool FeedBackBuffer2(void* p);
	};
}
