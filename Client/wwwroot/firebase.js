// importing from saved cause error wtf
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-app.js";
import { initializeAuth, browserLocalPersistence, signInAnonymously, getAuth } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-auth.js";
import { initializeFirestore, doc, onSnapshot } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-firestore.js";

const app = initializeApp({
    apiKey: "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg",
    authDomain: "godfield.firebaseapp.com",
    databaseURL: "https://godfield.firebaseio.com",
    projectId: "godfield"
});
const auth = initializeAuth(app, { persistence: browserLocalPersistence });
const db = initializeFirestore(app, { experimentalForceLongPolling: true });

const unsubs = {};

window.FirebaseSignIn = async () => await signInAnonymously(auth);

window.FirestoreSubscribe = (id, docPath, funcName) => {
    unsubs[id] =  onSnapshot(doc(db, docPath), snapshot => DotNet.invokeMethodAsync("GodOfGodField.Client", funcName, id, snapshot.exists() ? snapshot.data() : null));
    return id;
};

window.FirestoreUnsubscribe = (id) => {
    unsubs[id]();
    delete unsubs[id];
};