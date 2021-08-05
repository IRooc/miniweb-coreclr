const toggles = document.querySelectorAll('[data-bs-toggle]');
for (let i = 0; i < toggles.length; i++) {
	const toggle = toggles[i];
	toggle.addEventListener('click', () => {
		const className = toggle.dataset.bsToggle;
		const targets = document.querySelectorAll(toggle.dataset.bsTarget);
		targets
		for (let j = 0; j < targets.length; j++) {
			const target = targets[j];
			target
			if (target.classList.contains(className)) {
				target.classList.remove(className);
			} else {
				target.classList.add(className);
			}
		}
	});
}

