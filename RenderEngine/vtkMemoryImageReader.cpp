#include "vtkMemoryImageReader.h"

#include "vtkImageData.h"

#include "vtkByteSwap.h"
#include "vtkDataArray.h"
#include "vtkImageData.h"
#include "vtkInformation.h"
#include "vtkInformationVector.h"
#include "vtkObjectFactory.h"
#include "vtkPointData.h"
#include "vtkErrorCode.h"
#include "vtkStringArray.h"
#include <vtkStreamingDemandDrivenPipeline.h>
#include "vtksys/SystemTools.hxx"

vtkStandardNewMacro(vtkMemoryImageReader);

vtkMemoryImageReader::vtkMemoryImageReader()
{
	this->SetNumberOfInputPorts(0);
}


vtkMemoryImageReader::~vtkMemoryImageReader()
{
}

void vtkMemoryImageReader::SetImages(void ** images, int width, int height, int numberOfImages)
{
	this->images = images;
	this->width = width;
	this->height = height;
	this->numberOfImages = numberOfImages;
}

int vtkMemoryImageReader::RequestUpdateExtent(vtkInformation *, vtkInformationVector ** inputVector, vtkInformationVector * outputVector)
{
	return 1;
}

int vtkMemoryImageReader::RequestData(vtkInformation *, vtkInformationVector **, vtkInformationVector * outputVector)
{
	// get the data object
	vtkInformation *outInfo = outputVector->GetInformationObject(0);
	vtkImageData *output = vtkImageData::SafeDownCast(
		outInfo->Get(vtkDataObject::DATA_OBJECT()));

	this->Execute(output);

	return 1;
}

void vtkMemoryImageReader::Execute(vtkImageData * output)
{
	output->SetDimensions(width, height, numberOfImages);
	output->SetScalarType(VTK_UNSIGNED_SHORT, GetOutputInformation(0));	
	output->SetOrigin(0, 0, 0);
	output->SetNumberOfScalarComponents(1, GetOutputInformation(0));
	output->AllocateScalars(VTK_UNSIGNED_SHORT, 1);
	output->SetExtent(0, width - 1, 0, height - 1, 0, numberOfImages - 1);

	unsigned short* p = (unsigned short*)output->GetScalarPointer();
	for (int i = 0; i < numberOfImages; i++)
	{
		memcpy(p , images[i], width * height * 2);
		p = p + width * height;
	}	
	output->Modified();
}