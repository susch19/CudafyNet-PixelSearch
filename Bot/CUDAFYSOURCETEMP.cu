struct ProgramGPUColorRGB
{
	__device__  ProgramGPUColorRGB()
	{
	}
	unsigned char Blue;
	unsigned char Green;
	unsigned char Red;
	unsigned char Alpha;
};


// Insaniquarium_Deluxe_Bot.Program
extern "C" __global__  void FindPixel( ProgramGPUColorRGB* rgbColors, int rgbColorsLen0,  ProgramGPUColorRGB* colors, int colorsLen0,  int* indices, int indicesLen0,  float* output, int outputLen0);

// Insaniquarium_Deluxe_Bot.Program
extern "C" __global__  void FindPixel( ProgramGPUColorRGB* rgbColors, int rgbColorsLen0,  ProgramGPUColorRGB* colors, int colorsLen0,  int* indices, int indicesLen0,  float* output, int outputLen0)
{
	__syncthreads();
	indices[(threadIdx.x)]++;
}
