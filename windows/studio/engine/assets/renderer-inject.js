((cssText, artDataUrl) => {
  const STATE_KEY = "__CODEX_DREAM_SKIN_STATE__";
  const STYLE_ID = "codex-dream-skin-style";
  const CHROME_ID = "codex-dream-skin-chrome";
  const VERSION = "2.0.1";
  const DECORATION_VERSION = "2";
  const SEMANTIC_CLASSES = [
    "dream-start-title",
    "dream-start-screen",
    "dream-start-frame",
    "dream-start-composer",
    "dream-work-utility",
    "dream-mode-switch",
    "dream-mode-chat",
    "dream-mode-work",
    "dream-mode-active",
  ];

  window.__CODEX_DREAM_SKIN_DISABLED__ = false;

  const previous = window[STATE_KEY];
  previous?.observer?.disconnect();
  if (previous?.timer) clearInterval(previous.timer);
  if (previous?.scheduler?.timeout) clearTimeout(previous.scheduler.timeout);

  const artUrl = previous?.artUrl || (() => {
    const comma = artDataUrl.indexOf(",");
    const mimeType = artDataUrl.slice(5, artDataUrl.indexOf(";")) || "image/png";
    const binary = atob(artDataUrl.slice(comma + 1));
    const bytes = new Uint8Array(binary.length);
    for (let index = 0; index < binary.length; index += 1) bytes[index] = binary.charCodeAt(index);
    return URL.createObjectURL(new Blob([bytes], { type: mimeType }));
  })();

  const isVisible = (element) => {
    if (!element || element.classList.contains("invisible")) return false;
    const rect = element.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0 && getComputedStyle(element).visibility !== "hidden";
  };

  const clearSemanticClasses = () => {
    for (const className of SEMANTIC_CLASSES) {
      document.querySelectorAll(`.${className}`).forEach((element) => element.classList.remove(className));
    }
  };

  const syncExclusiveClass = (className, target) => {
    document.querySelectorAll(`.${className}`).forEach((element) => {
      if (element !== target) element.classList.remove(className);
    });
    target?.classList.add(className);
  };

  const clearSkinDom = () => {
    const root = document.documentElement;
    root?.classList.remove("codex-dream-skin");
    root?.style.removeProperty("--dream-art");
    clearSemanticClasses();
    document.querySelectorAll(
      ".dream-home, .dream-home-shell, .dream-start-shell, .dream-chat-start, .dream-work-start",
    ).forEach((node) => {
      node.classList.remove(
        "dream-home",
        "dream-home-shell",
        "dream-start-shell",
        "dream-chat-start",
        "dream-work-start",
      );
    });
    document.querySelectorAll(
      ".dream-miku-sidebar, .dream-nav-new, .dream-nav-pull, .dream-nav-scheduled, .dream-nav-skills, .dream-nav-sites, .dream-nav-chat, .dream-workspace-original, .dream-sidebar-footer-button",
    ).forEach((node) => {
      node.classList.remove(
        "dream-miku-sidebar",
        "dream-nav-new",
        "dream-nav-pull",
        "dream-nav-scheduled",
        "dream-nav-skills",
        "dream-nav-sites",
        "dream-nav-chat",
        "dream-workspace-original",
        "dream-sidebar-footer-button",
      );
    });
    document.getElementById(STYLE_ID)?.remove();
    document.getElementById(CHROME_ID)?.remove();
  };

  const ensure = () => {
    if (window.__CODEX_DREAM_SKIN_DISABLED__) return;
    const root = document.documentElement;
    if (!root || !document.body) return;

    const shellMain = document.querySelector("main.main-surface");
    const shellSidebar = document.querySelector("aside.app-shell-left-panel");
    if (!shellMain || !shellSidebar) {
      clearSkinDom();
      return;
    }

    root.classList.add("codex-dream-skin");
    root.style.setProperty("--dream-art", `url("${artUrl}")`);

    let style = document.getElementById(STYLE_ID);
    if (!style) {
      style = document.createElement("style");
      style.id = STYLE_ID;
      (document.head || root).appendChild(style);
    }
    if (style.dataset.dreamVersion !== DECORATION_VERSION || style.textContent !== cssText) {
      style.textContent = cssText;
      style.dataset.dreamVersion = DECORATION_VERSION;
    }

    const legacyHome = document.querySelector('[role="main"]:has([data-testid="home-icon"])');
    document.querySelectorAll('[role="main"].dream-home').forEach((candidate) => {
      if (candidate !== legacyHome) candidate.classList.remove("dream-home");
    });
    legacyHome?.classList.add("dream-home");
    shellMain.classList.toggle("dream-home-shell", Boolean(legacyHome));

    const sidebar = shellSidebar;
    if (sidebar) {
      sidebar.classList.add("dream-miku-sidebar");
      sidebar.querySelector(".dream-sidebar-brand")?.remove();
      sidebar.querySelectorAll(".dream-account-status").forEach((element) => element.classList.remove("dream-account-status"));

      const sidebarButtons = [...sidebar.querySelectorAll("button")];
      const navClassNames = [
        "dream-nav-new",
        "dream-nav-pull",
        "dream-nav-scheduled",
        "dream-nav-skills",
        "dream-nav-sites",
        "dream-nav-chat",
        "dream-workspace-original",
        "dream-sidebar-footer-button",
      ];
      const workspaceButton = sidebarButtons.find((button) => {
        const ariaLabel = button.getAttribute("aria-label") || "";
        return /切换模式|Switch mode/i.test(ariaLabel);
      });
      const navRules = [
        [/^(新聊天|新建任务|New chat|New task)(?:\s|$)/i, "dream-nav-new"],
        [/^(拉取请求|Pull requests?)$/i, "dream-nav-pull"],
        [/^(已安排|Scheduled)$/i, "dream-nav-scheduled"],
        [/^(插件|技能|Plugins?|Skills?)$/i, "dream-nav-skills"],
        [/^(站点|Sites?)$/i, "dream-nav-sites"],
        [/^(聊天|Chats?)$/i, "dream-nav-chat"],
      ];
      const sidebarBox = sidebar.getBoundingClientRect();
      for (const button of sidebarButtons) {
        const text = (button.textContent || "").trim().replace(/\s+/g, " ");
        const desiredNavClass = navRules.find(([pattern]) => pattern.test(text))?.[1] || null;
        const buttonBox = button.getBoundingClientRect();
        for (const className of navClassNames) {
          let shouldHaveClass = className === desiredNavClass;
          if (className === "dream-workspace-original") shouldHaveClass = button === workspaceButton;
          if (className === "dream-sidebar-footer-button") {
            shouldHaveClass = buttonBox.height > 0 && buttonBox.bottom >= sidebarBox.bottom - 64;
          }
          button.classList.toggle(className, shouldHaveClass);
        }
      }
    }

    const header = shellMain.querySelector(":scope > header.app-header-tint") || shellMain.querySelector("header.app-header-tint");
    const headerButtons = header ? [...header.querySelectorAll("button")] : [];
    const chatButton = headerButtons.find((button) => (button.textContent || "").trim() === "Chat");
    const workButton = headerButtons.find((button) => (button.textContent || "").trim() === "Work");
    let activeMode = "chat";
    let modeSwitch = null;

    if (chatButton && workButton && chatButton.parentElement === workButton.parentElement) {
      modeSwitch = chatButton.parentElement;
      activeMode = workButton.getAttribute("aria-pressed") === "true" ? "work" : "chat";
    }
    syncExclusiveClass("dream-mode-switch", modeSwitch);
    syncExclusiveClass("dream-mode-chat", chatButton || null);
    syncExclusiveClass("dream-mode-work", workButton || null);
    syncExclusiveClass("dream-mode-active", activeMode === "work" ? workButton : chatButton);

    const composer = shellMain.querySelector(".composer-surface-chrome");
    const startScreen = shellMain.querySelector('div[class*="container-name:home-main-content"]');
    const composerBox = composer?.getBoundingClientRect();
    const startTitle = startScreen && composerBox
      ? [...startScreen.querySelectorAll("h1, h2, h3, div, span")].find((element) => {
        if (!isVisible(element)) return false;
        const text = (element.textContent || "").trim().replace(/\s+/g, " ");
        if (text.length < 3 || text.length > 90) return false;
        const rect = element.getBoundingClientRect();
        if (rect.bottom >= composerBox.top - 18 || rect.top <= startScreen.getBoundingClientRect().top + 30) return false;
        const childRepeatsText = [...element.children].some((child) => (child.textContent || "").trim().replace(/\s+/g, " ") === text);
        return !childRepeatsText && Number.parseFloat(getComputedStyle(element).fontSize) >= 24;
      })
      : null;
    const startFrame = startScreen?.closest('div[class*="--thread-content-max-width"]') || null;
    const isCurrentStart = Boolean(!legacyHome && startScreen && startTitle && composer && startScreen.contains(composer));
    const workUtility = isCurrentStart ? startScreen.querySelector('[class*="_homeUtilityBar_"]') : null;

    shellMain.classList.toggle("dream-start-shell", isCurrentStart);
    shellMain.classList.toggle("dream-chat-start", isCurrentStart && activeMode === "chat");
    shellMain.classList.toggle("dream-work-start", isCurrentStart && activeMode === "work");
    syncExclusiveClass("dream-start-title", isCurrentStart ? startTitle : null);
    syncExclusiveClass("dream-start-screen", isCurrentStart ? startScreen : null);
    syncExclusiveClass("dream-start-frame", isCurrentStart ? startFrame : null);
    syncExclusiveClass("dream-start-composer", isCurrentStart ? composer : null);
    syncExclusiveClass("dream-work-utility", workUtility);

    let chrome = document.getElementById(CHROME_ID);
    if (!chrome || chrome.parentElement !== document.body) {
      chrome?.remove();
      chrome = document.createElement("div");
      chrome.id = CHROME_ID;
      chrome.setAttribute("aria-hidden", "true");
      document.body.appendChild(chrome);
    }
    if (chrome.dataset.dreamVersion !== DECORATION_VERSION) {
      chrome.innerHTML = `
        <div class="dream-brand"><span class="dream-note">♫</span><span><b>初音未来主题 Codex App</b><small>你的 AI 编程与创作伙伴 · 01</small></span></div>
        <div class="dream-signature">MIKU 01 ♡</div>
        <div class="dream-sparkles"><i></i><i></i><i></i><i></i><i></i><i></i></div>
        <div class="dream-ribbon"><span>♡</span>🎀<span>✦</span></div>
        <div class="dream-polaroid"></div>`;
      chrome.dataset.dreamVersion = DECORATION_VERSION;
    }

    const shellBox = shellMain.getBoundingClientRect();
    chrome.style.left = `${Math.round(shellBox.left)}px`;
    chrome.style.top = `${Math.round(shellBox.top)}px`;
    chrome.style.width = `${Math.round(shellBox.width)}px`;
    chrome.style.height = `${Math.round(shellBox.height)}px`;
    chrome.classList.toggle("dream-home-shell", Boolean(legacyHome));
    chrome.classList.toggle("dream-start-shell", isCurrentStart);
  };

  const cleanup = () => {
    window.__CODEX_DREAM_SKIN_DISABLED__ = true;
    clearSkinDom();
    const state = window[STATE_KEY];
    state?.observer?.disconnect();
    if (state?.timer) clearInterval(state.timer);
    if (state?.scheduler?.timeout) clearTimeout(state.scheduler.timeout);
    if (state?.artUrl) URL.revokeObjectURL(state.artUrl);
    delete window[STATE_KEY];
    return true;
  };

  const scheduler = { timeout: null };
  const scheduleEnsure = () => {
    if (scheduler.timeout) clearTimeout(scheduler.timeout);
    scheduler.timeout = setTimeout(() => {
      scheduler.timeout = null;
      ensure();
    }, 120);
  };
  const observer = new MutationObserver(scheduleEnsure);
  observer.observe(document.documentElement, { childList: true, subtree: true, attributes: true, attributeFilter: ["aria-pressed"] });
  const timer = setInterval(ensure, 5000);
  window[STATE_KEY] = { ensure, cleanup, observer, timer, scheduler, artUrl, version: VERSION };
  ensure();
  return { installed: true, version: VERSION };
})(__DREAM_CSS_JSON__, __DREAM_ART_JSON__)
