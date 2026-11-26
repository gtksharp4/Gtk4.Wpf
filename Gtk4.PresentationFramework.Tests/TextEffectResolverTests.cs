using System;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextEffectResolverTests
{
	static FlowDocument TwoRuns(out Run a, out Run b)
	{
		a = new Run("hello");
		b = new Run("world");
		var doc = new FlowDocument();
		var p = new Paragraph();
		p.Inlines.Add(a);
		p.Inlines.Add(b);
		doc.Blocks.Add(p);
		return doc;
	}

	[Fact]
	public void Resolve_rejects_null_arguments()
	{
		var doc = TwoRuns(out _, out _);
		var effect = new TextEffect();
		Assert.Throws<ArgumentNullException>(() => TextEffectResolver.Resolve(null!, doc.ContentEnd, effect));
		Assert.Throws<ArgumentNullException>(() => TextEffectResolver.Resolve(doc.ContentStart, null!, effect));
		Assert.Throws<ArgumentNullException>(() => TextEffectResolver.Resolve(doc.ContentStart, doc.ContentEnd, null!));
	}

	[Fact]
	public void Resolve_rejects_pointers_from_different_documents()
	{
		var one = TwoRuns(out _, out _);
		var two = TwoRuns(out _, out _);
		Assert.Throws<ArgumentException>(
			() => TextEffectResolver.Resolve(one.ContentStart, two.ContentEnd, new TextEffect()));
	}

	[Fact]
	public void Resolve_rejects_a_start_after_the_end()
	{
		var doc = TwoRuns(out _, out _);
		Assert.Throws<ArgumentException>(
			() => TextEffectResolver.Resolve(doc.ContentEnd, doc.ContentStart, new TextEffect()));
	}

	[Fact]
	public void Resolve_gives_one_target_per_run_with_positions_relative_to_the_run()
	{
		var doc = TwoRuns(out var a, out var b);
		var template = new TextEffect { PositionStart = 99, PositionCount = 99 };
		var targets = TextEffectResolver.Resolve(doc.ContentStart, doc.ContentEnd, template);
		Assert.Equal(2, targets.Length);
		Assert.Same(a, targets[0].Element);
		Assert.Same(b, targets[1].Element);
		Assert.All(targets, t => Assert.Equal(0, t.TextEffect.PositionStart));
		Assert.Equal(5, targets[0].TextEffect.PositionCount);
		Assert.Equal(5, targets[1].TextEffect.PositionCount);
		Assert.NotSame(template, targets[0].TextEffect);
		Assert.Equal(99, template.PositionStart);
	}

	[Fact]
	public void Resolve_clips_the_first_and_last_run_to_the_range()
	{
		var doc = TwoRuns(out _, out _);
		var start = doc.ContentStart.GetPositionAtOffset(3)!;
		var end = doc.ContentStart.GetPositionAtOffset(10)!;
		var targets = TextEffectResolver.Resolve(start, end, new TextEffect());
		Assert.Equal(2, targets.Length);
		Assert.Equal(1, targets[0].TextEffect.PositionStart);
		Assert.Equal(4, targets[0].TextEffect.PositionCount);
		Assert.Equal(0, targets[1].TextEffect.PositionStart);
		Assert.True(targets[1].TextEffect.PositionCount < 5);
	}

	[Fact]
	public void A_document_without_text_resolves_to_no_targets()
	{
		var doc = new FlowDocument();
		Assert.Empty(TextEffectResolver.Resolve(doc.ContentStart, doc.ContentEnd, new TextEffect()));
	}

	[Fact]
	public void Enable_adds_the_effect_and_Disable_removes_it()
	{
		var doc = TwoRuns(out var a, out _);
		var target = TextEffectResolver.Resolve(doc.ContentStart, doc.ContentEnd, new TextEffect())[0];
		Assert.False(target.IsEnabled);
		Assert.Null(a.TextEffects);
		target.Enable();
		target.Enable();
		Assert.True(target.IsEnabled);
		Assert.Single(a.TextEffects!);
		target.Disable();
		Assert.False(target.IsEnabled);
		Assert.Empty(a.TextEffects!);
	}

	[Fact]
	public void TextEffectTarget_validates_its_arguments()
	{
		Assert.Throws<ArgumentNullException>(() => new TextEffectTarget(null!, new TextEffect()));
		Assert.Throws<ArgumentNullException>(() => new TextEffectTarget(new Run(), null!));
	}

	[Fact]
	public void TextEffect_carries_transform_and_clip_and_clones_them()
	{
		var move = new TranslateTransform(5, 6);
		var effect = new TextEffect(null, move, 2, 3);
		Assert.Same(move, effect.Transform);
		Assert.Equal(2, effect.PositionStart);
		Assert.Equal(3, effect.PositionCount);
		Assert.Null(effect.Clip);
		Assert.NotNull(new TextEffect().Transform);
		// Transform is a Freezable, so Clone deep-copies it (WPF).
		var cloned = Assert.IsType<TranslateTransform>(((TextEffect)effect.Clone()).Transform);
		Assert.NotSame(move, cloned);
		Assert.Equal(5, cloned.X);
		Assert.Equal(6, cloned.Y);
	}
}
