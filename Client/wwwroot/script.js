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

async function PlayAudio(stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const audio = new Audio(url);
    audio.play();
    audio.onended = () => URL.revokeObjectURL(url);
}

function ShowMessage(message) {
    const messageElement = document.createElement("div");
    messageElement.style.position = "absolute";
    messageElement.style.top = "50%";
    messageElement.style.left = "50%";
    messageElement.style.transform = "translate(-50%, -50%)";
    messageElement.style.padding = "10px 20px";
    messageElement.style.backgroundColor = "rgba(238, 255, 238, 0.9)";
    messageElement.style.borderRadius = "10px";
    messageElement.style.boxShadow = "0 0 0 4px rgb(238, 255, 238), inset 0 0 0 2px rgb(0, 143, 111)";
    messageElement.style.userSelect = "none";
    messageElement.style.fontFamily = "sans-serif";
    messageElement.style.fontWeight = "bold";
    messageElement.style.fontSize = "60px";
    messageElement.innerHTML = message;
    document.body.appendChild(messageElement);
    setTimeout(() => messageElement.remove(), 2000);
}