﻿using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using Webprofusion.Scalex.Music;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scalex.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChordsPage : ContentPage
    {
        private Webprofusion.Scalex.Rendering.ChordDiagramRenderer chordDiagramRenderer;

        public ChordsPage()
        {
            InitializeComponent();

            chordDiagramRenderer = new Webprofusion.Scalex.Rendering.ChordDiagramRenderer();

            SkiaDrawingSurface.ApplyThemeColours(chordDiagramRenderer);

            var guitarModel = chordDiagramRenderer.GuitarModel;

            // tuning list
            this.tuningPicker.ItemsSource = guitarModel.AllTunings;
            this.tuningPicker.SelectedIndex = 0;

            // chord types
            List<ChordDefinition> chordGroups = new List<ChordDefinition>();
            chordGroups.Add(new ChordDefinition(ChordGroup.Common, "Common Guitar Chords", "popular", "popular", null));
            chordGroups.AddRange(chordDiagramRenderer.GuitarModel.GetAllChordDefinitions());
            typePicker.ItemsSource = chordGroups;
            this.typePicker.SelectedIndex = 0;

            SetPageTitle();
        }

        private void SetPageTitle()
        {
            var guitarModel = this.chordDiagramRenderer.GuitarModel;
            var chordType = (Webprofusion.Scalex.Music.ChordDefinition)typePicker.SelectedItem;
            this.Title = $"Chords -  {chordType?.Name} {guitarModel.SelectedTuning.Name }";
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            var skiaDrawingSurface = new SkiaDrawingSurface(surface.Canvas);

            int requiredHeight = chordDiagramRenderer.GetRequiredHeight(skiaDrawingSurface);
            if (this.SkiaCanvas.Height < requiredHeight)
            {
                // this.CanvasView.HeightRequest = requiredHeight;
            }

            // work out the width we need for our diagram
            var diagramRequiredWidth = chordDiagramRenderer.GetRequiredWidthPerChord();
            var canvasWidth = info.Width;

            var chordsPerRow = canvasWidth / diagramRequiredWidth;
            if (chordsPerRow > 10) chordsPerRow = 10;
            chordDiagramRenderer.ChordsPerRow = (int)chordsPerRow - 1;

            float scaleFactor = (float)canvasWidth / (float)(diagramRequiredWidth * chordsPerRow);
            if (scaleFactor < 1.5) scaleFactor = 1.5f;

            skiaDrawingSurface.SetScale((float)scaleFactor);

            canvas.Clear();

            chordDiagramRenderer.Render(skiaDrawingSurface);
        }

        private void tuningPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tuningPicker.SelectedIndex > -1)
            {
                var tuning = (Webprofusion.Scalex.Guitar.GuitarTuning)tuningPicker.SelectedItem;
                this.chordDiagramRenderer.GuitarModel.SetTuning(tuning.ID);
                this.SkiaCanvas.InvalidateSurface();
                SetPageTitle();
            }
        }

        private void typePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (typePicker.SelectedIndex > -1)
            {
                var chordType = (Webprofusion.Scalex.Music.ChordDefinition)typePicker.SelectedItem;

                chordDiagramRenderer.CurrentChordDiagrams = chordDiagramRenderer.GuitarModel.GetChordDiagramsByGroup(chordType.ShortName);
                SkiaCanvas.InvalidateSurface();
                SetPageTitle();
            }
        }
    }
}