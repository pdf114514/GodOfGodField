HTMLCollection.prototype.at = function(index) { return this[index]; }

async function SetImage(elementRef, stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    elementRef.onload = () => URL.revokeObjectURL(url);
    elementRef.src = url;
}

async function SetBackgroundImage(elementRef, stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const image = document.createElement("img");
    image.onload = () => {
        elementRef.style.backgroundImage = `url(\"${url}\")`;
        setTimeout(() => URL.revokeObjectURL(url), 100);
    }
    image.src = url;
}