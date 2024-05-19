// defining these functions causes error asf
// Object.prototype.getProperty = k => this[k];
// Object.prototype.setProperty = (k, v) => this[k] = v;

Object.defineProperty(Object.prototype, "getProperty", {
    value: function(key) { return this[key]; },
    writable: true,
    configurable: true,
    enumerable: false
});
Object.defineProperty(Object.prototype, "setProperty", {
    value: function(key, value) { this[key] = value; },
    writable: true,
    configurable: true,
    enumerable: false
});


HTMLCollection.prototype.at = function(index) { return this[index]; }

async function SetImage(elementRef, stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    elementRef.onload = () => URL.revokeObjectURL(url);
    elementRef.src = url;
}

async function CreateObjectURL(stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    return URL.createObjectURL(blob);
}

async function CreateImageFromURL(url) {
    // wait for the image to load
    return await new Promise((resolve, reject) => {
        const image = new Image();
        image.onload = () => resolve(image);
        image.onerror = reject;
        image.src = url;
    });
}

function RevokeObjectURL(url) {
    URL.revokeObjectURL(url);
}

function RemoveImage(elementRef) {
    elementRef.src = "";
}

async function SetBackgroundImage(elementRef, stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const image = document.createElement("img");
    image.onload = () => {
        elementRef.style.backgroundImage = `url(\"${url}\")`;
        setTimeout(() => URL.revokeObjectURL(url), 100);
        image.remove();
    }
    image.src = url;
}

async function PlayAudio(stream) {
    const arrayBuffer = await stream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const audio = new Audio(url);
    audio.play();
    audio.onended = () => {
        URL.revokeObjectURL(url);
        audio.remove();
    }
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

function EnqueueEvent(eventName, eventArgs = {}) {
    const data = {
        "action": {
            "stringValue": eventName
        },
        ...eventArgs
    };
    return DotNet.invokeMethodAsync("GodOfGodField.Client", "EnqueueEvent", JSON.stringify(data));
}

// add fps control
function main() {
    document.addEventListener("selectionchange", async () => {
        await DotNet.invokeMethodAsync("GodOfGodField.Client", "SelectionChange");
    });
    const canvas = document.getElementById("Game");
    let now = performance.now();
    let windowResized = true;
    window.addEventListener("resize", () => windowResized = true);
    const adjustCanvasSize = () => {
        if (windowResized) {
            windowResized = false;
            let scale = window.devicePixelRatio || 1;
            // let width = window.innerWidth;
            // let height = window.innerHeight;
            let width = document.documentElement.clientWidth;
            let height = document.documentElement.clientHeight;
            canvas.width = width * scale;
            canvas.height = height * scale;
            // canvas.style.width = width + "px";
            // canvas.style.height = height + "px";
            canvas.style.width = "100%";
            canvas.style.height = "100%";
        }
    };
    const animate = async time => {
        const resized = windowResized;
        const dt = time - now;
        now = time;
        adjustCanvasSize();
        try {
            await DotNet.invokeMethodAsync("GodOfGodField.Client", "Render", time, resized, dt, 1000 / dt);
        } catch (e) {
            console.error(e);
            alert(e);
        }
        setTimeout(() => requestAnimationFrame(animate), 1);
        // requestAnimationFrame(animate);
    };
    requestAnimationFrame(animate);
}