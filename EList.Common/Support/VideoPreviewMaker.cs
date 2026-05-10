//using Microsoft.AspNetCore.Mvc.Formatters;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace EList.Common.Support
//{
//    internal class VideoPreviewMaker
//    {
//    }
//}

using FFMpegCore..AutoGen;
using System.Runtime.InteropServices;

public unsafe class DirectStreamThumbnailExtractor : IDisposable
{
    private bool _initialized = false;

    public DirectStreamThumbnailExtractor()
    {
        // Инициализация FFmpeg — вызывается один раз при запуске приложения
        if (!_initialized)
        {
            ffmpeg.avformat_network_init();
            ffmpeg.avcodec_register_all();
            _initialized = true;
        }
    }

    public async Task<byte[]> ExtractThumbnailAsync(Stream videoStream,
        int width = 320, TimeSpan? position = null)
    {
        var seekPosition = position ?? TimeSpan.FromSeconds(1);

        // Копируем поток в MemoryStream для многократного чтения
        using var memoryStream = new MemoryStream();
        await videoStream.CopyToAsync(memoryStream);
        byte[] videoData = memoryStream.ToArray();

        AVFormatContext* formatContext = null;
        AVCodecContext* codecContext = null;
        AVFrame* frame = null;
        AVFrame* rgbFrame = null;
        SwsContext* swsContext = null;

        try
        {
            // Открываем видео из памяти
            var ioContext = CreateCustomIOContext(videoData);
            formatContext = ffmpeg.avformat_alloc_context();
            formatContext->pb = ioContext;
            formatContext->flags |= ffmpeg.AVFMT_FLAG_CUSTOM_IO;

            if (ffmpeg.avformat_open_input(&formatContext, "memory", null, null) < 0)
                throw new Exception("Не удалось открыть видео");

            if (ffmpeg.avformat_find_stream_info(formatContext, null) < 0)
                throw new Exception("Не удалось найти информацию о потоках");

            // Находим видеопоток
            int videoStreamIndex = -1;
            for (int i = 0; i < formatContext->nb_streams; i++)
            {
                if (formatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    videoStreamIndex = i;
                    break;
                }
            }

            if (videoStreamIndex == -1)
                throw new Exception("Видеопоток не найден");

            // Создаем декодер
            AVCodec* codec = ffmpeg.avcodec_find_decoder(
                formatContext->streams[videoStreamIndex]->codecpar->codec_id);
            if (codec == null)
                throw new Exception("Кодек не найден");

            codecContext = ffmpeg.avcodec_alloc_context3(codec);
            ffmpeg.avcodec_parameters_to_context(codecContext,
                formatContext->streams[videoStreamIndex]->codecpar);
            ffmpeg.avcodec_open2(codecContext, codec, null);

            // Перемещаемся к нужной позиции
            long seekTarget = (long)(seekPosition.TotalSeconds * AV_TIME_BASE);
            ffmpeg.av_seek_frame(formatContext, videoStreamIndex, seekTarget,
                ffmpeg.AVSEEK_FLAG_BACKWARD);

            // Подготавливаем кадры
            frame = ffmpeg.av_frame_alloc();
            rgbFrame = ffmpeg.av_frame_alloc();

            int targetWidth = width;
            int targetHeight = width * codecContext->height / codecContext->width;

            int byteCount = ffmpeg.av_image_get_buffer_size(
                ffmpeg.AV_PIX_FMT_RGB24, targetWidth, targetHeight, 1);
            byte* buffer = (byte*)ffmpeg.av_malloc(byteCount);

            ffmpeg.av_image_fill_arrays(rgbFrame->data, rgbFrame->linesize, buffer,
                ffmpeg.AV_PIX_FMT_RGB24, targetWidth, targetHeight, 1);

            // Создаем контекст для конвертации цветов
            swsContext = ffmpeg.sws_getContext(
                codecContext->width, codecContext->height, codecContext->pix_fmt,
                targetWidth, targetHeight, ffmpeg.AV_PIX_FMT_RGB24,
                ffmpeg.SWS_BILINEAR, null, null, null);

            // Читаем и декодируем кадры
            AVPacket* packet = ffmpeg.av_packet_alloc();
            bool frameExtracted = false;

            while (!frameExtracted && ffmpeg.av_read_frame(formatContext, packet) >= 0)
            {
                if (packet->stream_index == videoStreamIndex)
                {
                    ffmpeg.avcodec_send_packet(codecContext, packet);
                    if (ffmpeg.avcodec_receive_frame(codecContext, frame) == 0)
                    {
                        // Конвертируем в RGB
                        ffmpeg.sws_scale(swsContext,
                            frame->data, frame->linesize, 0, codecContext->height,
                            rgbFrame->data, rgbFrame->linesize);
                        frameExtracted = true;
                    }
                }
                ffmpeg.av_packet_unref(packet);
            }

            if (!frameExtracted)
                throw new Exception("Не удалось извлечь кадр");

            // Конвертируем в PNG (у FFmpeg.AutoGen нет встроенного энкодера PNG)
            // Здесь нужно дополнительное преобразование или библиотека для сохранения

            ffmpeg.av_packet_free(&packet);

            // В реальном проекте здесь нужно преобразовать rgbFrame в PNG
            // через System.Drawing или аналоги
            return ConvertRGBToPng(rgbFrame, targetWidth, targetHeight, buffer);
        }
        finally
        {
            // Освобождение ресурсов
            if (swsContext != null) ffmpeg.sws_freeContext(swsContext);
            if (rgbFrame != null) ffmpeg.av_frame_free(&rgbFrame);
            if (frame != null) ffmpeg.av_frame_free(&frame);
            if (codecContext != null) ffmpeg.avcodec_free_context(&codecContext);
            if (formatContext != null) ffmpeg.avformat_close_input(&formatContext);
        }
    }

    // Создание кастомного IO контекста для чтения из памяти
    private AVIOContext* CreateCustomIOContext(byte[] data)
    {
        // Реализация требует Pin объекта и настройки callback'ов
        // Это нетривиальная часть, здесь опущена для краткости
        throw new NotImplementedException("Требуется дополнительная реализация");
    }

    private byte[] ConvertRGBToPng(AVFrame* frame, int width, int height, byte* buffer)
    {
        // Копируем данные из неуправляемой памяти и конвертируем в PNG
        // Для этого нужно использовать System.Drawing или ImageSharp
        byte[] result = new byte[width * height * 3];
        Marshal.Copy((IntPtr)buffer, result, 0, result.Length);

        // Здесь добавить конвертацию в PNG
        return result;
    }
}
