// Keeps the active glyph in view inside the fixed-height passage window, so long
// passages scroll line-by-line as you type instead of running off-screen.
// Uses a CSS transform on the passage (reliable regardless of overflow/scroll
// quirks) rather than scrollTop. Called from Play.razor after each render.
window.qwertide = {
  followCaret: function () {
    var caret = document.getElementById("q-caret");
    var vp = document.getElementById("q-passage-viewport");
    if (!caret || !vp) return;
    var passage = vp.querySelector(".q-passage");
    if (!passage) return;

    // Center the caret's line within the visible window, clamped so we never
    // translate above the first line or past the last.
    var offset = caret.offsetTop - (vp.clientHeight / 2) + (caret.offsetHeight / 2);
    var max = passage.offsetHeight - vp.clientHeight;
    if (max < 0) max = 0;
    offset = Math.max(0, Math.min(offset, max));

    passage.style.transform = "translateY(" + (-offset) + "px)";
  }
};
